#include "Direct3D12Content.h"
#include "Direct3D12Core.h"
#include "Utilities/IOStream.h"
#include "Content/ContentToEngine.h"
#include "Direct3D12GPass.h"

#ifdef OPAQUE
#undef OPAQUE
#endif

#ifdef TRANSPARENT
#undef TRANSPARENT
#endif

namespace lightning::graphics::direct3d12::content {
	namespace {
		struct SubmeshView {
			D3D12_VERTEX_BUFFER_VIEW position_buffer_view{};
			D3D12_VERTEX_BUFFER_VIEW element_buffer_view{};
			D3D12_INDEX_BUFFER_VIEW index_buffer_view{};
			D3D_PRIMITIVE_TOPOLOGY primitive_topology{};
			u32 element_type{};
		};

		util::free_list<ID3D12Resource*> submesh_buffers{};
		util::free_list<SubmeshView> submesh_views{};
		std::mutex submesh_mutex{};

		util::free_list<D3D12Texture> textures;
		std::mutex texture_mutex{};

		util::vector<ID3D12RootSignature*> root_signatures;
		std::unordered_map<u64, id::id_type> material_rs_map;
		util::free_list<std::unique_ptr<u8[]>> materials;
		std::mutex material_mutex{};

		constexpr D3D12_ROOT_SIGNATURE_FLAGS get_root_signature_flags(ShaderFlags::Flags flags) {
			D3D12_ROOT_SIGNATURE_FLAGS default_flags{ d3dx::D3D12RootSignatureDesc::default_flags };

			if (flags & ShaderFlags::VERTEX) default_flags &= ~D3D12_ROOT_SIGNATURE_FLAG_DENY_VERTEX_SHADER_ROOT_ACCESS;
			if (flags & ShaderFlags::HULL) default_flags &= ~D3D12_ROOT_SIGNATURE_FLAG_DENY_HULL_SHADER_ROOT_ACCESS;
			if (flags & ShaderFlags::DOMAIN) default_flags &= ~D3D12_ROOT_SIGNATURE_FLAG_DENY_DOMAIN_SHADER_ROOT_ACCESS;
			if (flags & ShaderFlags::GEOMETRY) default_flags &= ~D3D12_ROOT_SIGNATURE_FLAG_DENY_GEOMETRY_SHADER_ROOT_ACCESS;
			if (flags & ShaderFlags::PIXEL) default_flags &= ~D3D12_ROOT_SIGNATURE_FLAG_DENY_PIXEL_SHADER_ROOT_ACCESS;
			if (flags & ShaderFlags::AMPLIFICATION) default_flags &= ~D3D12_ROOT_SIGNATURE_FLAG_DENY_AMPLIFICATION_SHADER_ROOT_ACCESS;
			if (flags & ShaderFlags::MESH) default_flags &= ~D3D12_ROOT_SIGNATURE_FLAG_DENY_MESH_SHADER_ROOT_ACCESS;

			return default_flags;
		}

		id::id_type create_root_signature(MaterialType::Type type, ShaderFlags::Flags flags) {
			assert(type < MaterialType::count);
			static_assert(sizeof(type) == sizeof(u32) && sizeof(flags) == sizeof(u32));
			const u64 key{ ((u64)type << 32) | flags };
			auto pair = material_rs_map.find(key);

			if (pair != material_rs_map.end()) {
				assert(pair->first == key);
				return pair->second;
			}

			ID3D12RootSignature* root_signature{ nullptr };

			switch (type) {
				case MaterialType::Type::OPAQUE:
					using params = gpass::OpaqueRootParameter;
					d3dx::D3D12RootParameter parameters[params::count]{};
					parameters[params::PER_FRAME_DATA].as_cbv(D3D12_SHADER_VISIBILITY_ALL, 0);

					D3D12_SHADER_VISIBILITY buffer_visibility{};
					D3D12_SHADER_VISIBILITY data_visibility{};

					if (flags & ShaderFlags::VERTEX) {
						buffer_visibility = D3D12_SHADER_VISIBILITY_VERTEX;
						data_visibility = D3D12_SHADER_VISIBILITY_VERTEX;
					}
					else if (flags & ShaderFlags::MESH) {
						buffer_visibility = D3D12_SHADER_VISIBILITY_MESH;
						data_visibility = D3D12_SHADER_VISIBILITY_MESH;
					}

					if ((flags & ShaderFlags::HULL) || (flags & ShaderFlags::GEOMETRY) || (flags & ShaderFlags::AMPLIFICATION)) {
						buffer_visibility = D3D12_SHADER_VISIBILITY_ALL;
						data_visibility = D3D12_SHADER_VISIBILITY_ALL;
					}

					if ((flags & ShaderFlags::PIXEL) || (flags & ShaderFlags::COMPUTE)) {
						data_visibility = D3D12_SHADER_VISIBILITY_ALL;
					}

					parameters[params::POSITION_BUFFER].as_srv(buffer_visibility, 0);
					parameters[params::ELEMENT_BUFFER].as_srv(buffer_visibility, 1);
					parameters[params::SRV_INDICIES].as_srv(D3D12_SHADER_VISIBILITY_PIXEL, 2);
					parameters[params::PER_OBJECT_DATA].as_cbv(data_visibility, 1);

					root_signature = d3dx::D3D12RootSignatureDesc{ &parameters[0], _countof(parameters), get_root_signature_flags(flags) }.create();

					break;
			}

			assert(root_signature);
			const id::id_type id{ (id::id_type)root_signatures.size() };
			root_signatures.emplace_back(root_signature);
			material_rs_map[key] = id;
			NAME_D3D12_OBJECT_INDEXED(root_signature, key, L"GPass Root Signature - key");

			return id;
		}

		class D3D12MaterialStream {
			public:
				DISABLE_COPY_AND_MOVE(D3D12MaterialStream);
				explicit D3D12MaterialStream(u8* const material_buffer) : _buffer{ material_buffer } {
					initialize();
				}

				explicit D3D12MaterialStream(std::unique_ptr<u8[]>& material_buffer, MaterialInitInfo info) {
					assert(!material_buffer);

					u32 shader_count{ 0 };
					u32 flags{ 0 };
					for (u32 i{ 0 }; i < ShaderType::count; ++i) {
						if (id::is_valid(info.shader_ids[i])) {
							++shader_count;
							flags |= (1 << i);
						}
					}

					assert(shader_count && flags);

					const u32 buffer_size{
						sizeof(MaterialType::Type) +								// material type
						sizeof(ShaderFlags::Flags) +								// shader flags
						sizeof(id::id_type) +										// root signature id
						sizeof(u32) +												// texture count
						sizeof(id::id_type) * shader_count +						// shader ids
						(sizeof(id::id_type) + sizeof(u32)) * info.texture_count	// texture ids and descriptor indicies 
					};

					material_buffer = std::make_unique<u8[]>(buffer_size);
					_buffer = material_buffer.get();
					u8* const buffer{ _buffer };

					*(MaterialType::Type*)buffer = info.type;
					*(ShaderFlags::Flags*)(&buffer[shader_flags_index]) = (ShaderFlags::Flags)flags;
					*(id::id_type*)(&buffer[root_signature_index]) = create_root_signature(info.type, (ShaderFlags::Flags)flags);
					*(u32*)(&buffer[texture_count_index]) = info.texture_count;

					initialize();

					if (info.texture_count) {
						memcpy(_texture_ids, info.texture_ids, info.texture_count * sizeof(id::id_type));
						texture::get_descriptor_indicies(_texture_ids, info.texture_count, _descriptor_indicies);
					}

					u32 shader_index{ 0 };
					for (u32 i{ 0 }; i < ShaderType::count; ++i) {
						if (id::is_valid(info.shader_ids[i])) {
							_shader_ids[shader_index] = info.shader_ids[i];
							++shader_index;
						}
					}

					assert(shader_index == _mm_popcnt_u32(_shader_flags));
 				}

				[[nodiscard]] constexpr u32 texture_count() const { return _texture_count; }
				[[nodiscard]] constexpr MaterialType::Type type() const { return _type; }
				[[nodiscard]] constexpr ShaderFlags::Flags shader_flags() const { return _shader_flags; }
				[[nodiscard]] constexpr id::id_type root_signature_id() const { return _root_signature_id; }
				[[nodiscard]] constexpr id::id_type* texture_ids() const { return _texture_ids; }
				[[nodiscard]] constexpr u32* descriptor_indicies() const { return _descriptor_indicies; }
				[[nodiscard]] constexpr id::id_type* shader_ids() const { return _shader_ids; }

			private:
				void initialize() {
					assert(_buffer);

					u8* const buffer{ _buffer };

					_type = *(MaterialType::Type*)buffer;
					_shader_flags = *(ShaderFlags::Flags*)(&buffer[shader_flags_index]);
					_root_signature_id = *(id::id_type*)(&buffer[root_signature_index]);
					_texture_count = *(u32*)(&buffer[texture_count_index]);

					_shader_ids = (id::id_type*)(&buffer[texture_count_index + sizeof(u32)]);
					_texture_ids = _texture_count ? &_shader_ids[_mm_popcnt_u32(_shader_flags)] : nullptr;
					_descriptor_indicies = _texture_count ? (u32*)(&_texture_ids[_texture_count]) : nullptr;
				}

				constexpr static u32 shader_flags_index{ sizeof(MaterialType::Type) };
				constexpr static u32 root_signature_index{ shader_flags_index + sizeof(ShaderFlags::Flags) };
				constexpr static u32 texture_count_index{ root_signature_index + sizeof(id::id_type) };

				u8* _buffer;
				id::id_type* _texture_ids;
				u32* _descriptor_indicies;
				id::id_type* _shader_ids;
				id::id_type _root_signature_id;
				u32 _texture_count;
				MaterialType::Type _type;
				ShaderFlags::Flags _shader_flags;
		};

		D3D_PRIMITIVE_TOPOLOGY get_d3d_primitive_topology(PrimitiveTopology::Type type) {

			assert(type < PrimitiveTopology::count);

			switch (type) {
				case PrimitiveTopology::Type::POINT_LIST:return D3D_PRIMITIVE_TOPOLOGY_POINTLIST;
				case PrimitiveTopology::Type::LINE_LIST:return D3D_PRIMITIVE_TOPOLOGY_LINELIST;
				case PrimitiveTopology::Type::LINE_STRIP:return D3D_PRIMITIVE_TOPOLOGY_LINESTRIP;
				case PrimitiveTopology::Type::TRIANGLE_LIST:return D3D_PRIMITIVE_TOPOLOGY_TRIANGLELIST;
				case PrimitiveTopology::Type::TRIANGLE_STRIP:return D3D_PRIMITIVE_TOPOLOGY_TRIANGLESTRIP;
			}
			return D3D_PRIMITIVE_TOPOLOGY_UNDEFINED;
		}
	}

	bool initialize() { return true; }

	void shutdown() {
		for (auto& item : root_signatures) {
			core::release(item);
		}
	}

	namespace submesh {
		id::id_type add(const u8*& data) {
			util::BlobStreamReader blob{ (const u8*)data };

			const u32 element_size{ blob.read<u32>()};
			const u32 vertex_count{ blob.read<u32>() };
			const u32 index_count{ blob.read<u32>() };
			const u32 elements_type{ blob.read<u32>() };
			const u32 primitive_topology{ blob.read<u32>() };
			const u32 index_size{ (vertex_count < (1 << 16)) ? sizeof(u16) : sizeof(u32) };

			const u32 position_buffer_size{ sizeof(math::v3) * vertex_count };
			const u32 element_buffer_size{ element_size * vertex_count };
			const u32 index_buffer_size{ index_size * index_count };

			constexpr u32 alignment{ D3D12_STANDARD_MAXIMUM_ELEMENT_ALIGNMENT_BYTE_MULTIPLE };
			const u32 aligned_position_buffer_size{ (u32)math::align_size_up<alignment>(position_buffer_size) };
			const u32 aligned_element_buffer_size{ (u32)math::align_size_up<alignment>(element_buffer_size) };
			const u32 total_buffer_size{ aligned_position_buffer_size + aligned_element_buffer_size + index_buffer_size };

			ID3D12Resource* resource{ d3dx::create_buffer(blob.position(), total_buffer_size) };

			blob.skip(total_buffer_size);
			data = blob.position();

			SubmeshView view{};
			view.position_buffer_view.BufferLocation = resource->GetGPUVirtualAddress();
			view.position_buffer_view.SizeInBytes = position_buffer_size;
			view.position_buffer_view.StrideInBytes = sizeof(math::v3);

			if (element_size) {
				view.element_buffer_view.BufferLocation = resource->GetGPUVirtualAddress() + aligned_position_buffer_size;
				view.element_buffer_view.SizeInBytes = element_buffer_size;
				view.element_buffer_view.StrideInBytes = element_size;
			}

			view.index_buffer_view.BufferLocation = resource->GetGPUVirtualAddress() + aligned_position_buffer_size + aligned_element_buffer_size;
			view.index_buffer_view.Format = (index_size == sizeof(u16)) ? DXGI_FORMAT_R16_UINT : DXGI_FORMAT_R32_UINT;
			view.index_buffer_view.SizeInBytes = index_buffer_size;

			view.element_type = elements_type;
			view.primitive_topology = get_d3d_primitive_topology((PrimitiveTopology::Type)primitive_topology);

			std::lock_guard lock{ submesh_mutex };
			submesh_buffers.add(resource);
			return submesh_views.add(view);
		} 

		void remove(id::id_type id) {
			std::lock_guard lock{ submesh_mutex };
			submesh_views.remove(id);

			core::deferred_release(submesh_buffers[id]);
			submesh_buffers.remove(id);
		}
	}

	namespace texture {
		void get_descriptor_indicies(const id::id_type* const texture_ids, u32 id_count, u32* const indicies) {
			assert(texture_ids && id_count && indicies);

			std::lock_guard lock{ texture_mutex };

			for (u32 i{ 0 }; i < id_count; ++i) {
				indicies[i] = textures[i].srv().index;
			}
		}
	}

	namespace material {
		id::id_type add(MaterialInitInfo info) {
			std::unique_ptr<u8[]> buffer;
			std::lock_guard lock{ material_mutex };

			D3D12MaterialStream stream{ buffer, info };

			assert(buffer);
			return materials.add(std::move(buffer));
		}

		void remove(id::id_type id) {
			std::lock_guard lock{ material_mutex };
			materials.remove(id);
		}
	}
}