#include "ContentToEngine.h"
#include "Graphics/Renderer.h"
#include "Utilities/IOStream.h"

namespace lightning::content {
	namespace {

		class GeometryHierarchyStream {
			public:
				struct LodOffset {
					u16 offset;
					u16 count;
				 };

				DISABLE_COPY_AND_MOVE(GeometryHierarchyStream);
				GeometryHierarchyStream(u8* const buffer, u32 lods = u32_invalid_id) : _buffer{ buffer } {
					assert(buffer && lods);
					if (lods != u32_invalid_id) *((u32*)buffer) = lods;
					_lod_count = *((u32*)buffer);
					_thresholds = (f32*)(&buffer[sizeof(u32)]);
					_lod_offsets = (LodOffset*)(&_thresholds[_lod_count]);
					_gpu_ids = (id::id_type*)(&_lod_offsets[_lod_count]);
				}

				void gpu_ids(u32 lod, id::id_type*& ids, u32& id_count) {
					assert(lod < _lod_count);
					ids = &_gpu_ids[_lod_offsets[lod].offset];
					id_count = _lod_offsets[lod].count;
				}

				u32 lod_from_threshold(f32 threshold) {
					assert(threshold > 0);

					for (u32 i{ _lod_count - 1 }; i > 0; --i) {
						if (_thresholds[i] <= threshold) return i;
					}

					assert(false);
					return 0;
				}

				[[nodiscard]] constexpr u32 lod_count() const { return _lod_count; }
				[[nodiscard]] constexpr f32* thresholds() const { return _thresholds; }
				[[nodiscard]] constexpr LodOffset* lod_offsets() const { return _lod_offsets; }
				[[nodiscard]] constexpr id::id_type* gpu_ids() const { return _gpu_ids; }

			private:
				u8* const _buffer;
				f32* _thresholds;
				LodOffset* _lod_offsets;
				id::id_type* _gpu_ids;
				u32 _lod_count;
		};

		util::free_list<u8*> geometry_hierarchies;
		std::mutex geometry_mutex;

		u32 get_geometry_hierarchy_buffer_size(const void* const data) {
			assert(data);
			util::BlobStreamReader blob{ (const u8*)data };
			const u32 lod_count{ blob.read<u32>() };
			assert(lod_count);

			u32 size{ sizeof(u32) + (sizeof(f32) + sizeof(GeometryHierarchyStream::LodOffset)) * lod_count };

			for (u32 lod_idx{ 0 }; lod_idx < lod_count; ++lod_idx) {
				blob.skip(sizeof(f32));
				size += sizeof(id::id_type) * blob.read<u32>();
				blob.skip(blob.read<u32>());
			}

			return size;
		}

		id::id_type create_mesh_hierarchy(const void* const data) {
			assert(data);
			const u32 size{ get_geometry_hierarchy_buffer_size(data) };
			u8* const hierarchy_buffer{ (u8* const)malloc(size) };

			util::BlobStreamReader blob{ (const u8*)data };
			const u32 lod_count{ blob.read<u32>() };
			assert(lod_count);
			GeometryHierarchyStream stream{ hierarchy_buffer, lod_count };
			u16 submesh_index{ 0 };
			id::id_type* const gpu_ids{ stream.gpu_ids() };

			for (u32 lod_idx{ 0 }; lod_idx < lod_count; ++lod_idx) {
				stream.thresholds()[lod_idx] = blob.read<f32>();
				const u32 id_count{ blob.read<u32>() };
				assert(id_count < (1 << 16));
				stream.lod_offsets()[lod_idx] = { submesh_index, (u16)id_count };
				blob.skip(sizeof(u32));
				for (u32 id_idx{ 0 }; id_idx < id_count; ++id_idx) {
					const u8* at{ blob.position() };
					gpu_ids[submesh_index++] = graphics::add_submesh(at);
					blob.skip((u32)(at - blob.position()));
					assert(submesh_index < (1 << 16));
				}
			}

			assert([&]() {
				f32 previous_threshold{ stream.thresholds()[0] };
				for (u32 i{ 1 }; i < lod_count; ++i) {
					if (stream.thresholds()[i] <= previous_threshold) return false;
					previous_threshold = stream.thresholds()[i];
				}
				return true;
			}());

			std::lock_guard lock{ geometry_mutex };
			return geometry_hierarchies.add(hierarchy_buffer);
		}

		id::id_type create_geometry_resource(const void* const data) { return create_mesh_hierarchy(data); }

		void destroy_geometry_resource(id::id_type id) {
			std::lock_guard lock{ geometry_mutex };
			u8* const pointer{ geometry_hierarchies[id] };

			GeometryHierarchyStream stream{ pointer };
			const u32 lod_count{ stream.lod_count() };
			u32 id_index{ 0 };

			for (u32 lod{ 0 }; lod < lod_count; ++lod) {
				for (u32 i{ 0 }; i < stream.lod_offsets()[lod].count; ++i) {
					graphics::remove_submesh(stream.gpu_ids()[id_index++]);
				}
			}

			free(pointer);
			geometry_hierarchies.remove(id);
		}
	}

	id::id_type create_resource(const void* const data, AssetType::Type type) {
		assert(data);
		id::id_type id{ id::invalid_id };

		switch (type) {
			case AssetType::ANIMATION:
				break;
			case AssetType::AUDIO:
				break;
			case AssetType::MATERIAL:
				break;
			case AssetType::MESH:
				id = create_geometry_resource(data);
				break;
			case AssetType::SKELETON:
				break;
			case AssetType::TEXTURE:
				break;
		}

		assert(id::is_valid(id));
		return id;
	}

	void destroy_resource(id::id_type id, AssetType::Type type) {
		assert(id::is_valid(id));

		switch (type) {
			case AssetType::ANIMATION:
				break;
			case AssetType::AUDIO:
				break;
			case AssetType::MATERIAL:
				break;
			case AssetType::MESH:
				destroy_geometry_resource(id);
				break;
			case AssetType::SKELETON:
				break;
			case AssetType::TEXTURE:
				break;
			default:
				assert(false);
				break;
		}
	}
}