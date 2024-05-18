#include "Geometry.h"

namespace lightning::tools {
	namespace {

		using namespace math;
		using namespace DirectX;

		void recalculate_normals(Mesh& m) {
			const u32 num_indicies{ (u32)m.raw_indicies.size() };
			m.normals.reserve(num_indicies);

			for (u32 i{ 0 }; i < num_indicies; ++i) {
				const u32 i0{ m.raw_indicies[i] };
				const u32 i1{ m.raw_indicies[++i] };
				const u32 i2{ m.raw_indicies[++i] };

				XMVECTOR v0{ XMLoadFloat3(&m.positions[i0]) };
				XMVECTOR v1{ XMLoadFloat3(&m.positions[i1]) };
				XMVECTOR v2{ XMLoadFloat3(&m.positions[i2]) };

				XMVECTOR e0{ v1 - v0 };
				XMVECTOR e1{ v2 - v0 };

				XMVECTOR n{ XMVector3Normalize(XMVector3Cross(e0, e1)) };

				XMStoreFloat3(&m.normals[i], n);
				m.normals[i - 1] = m.normals[i];
				m.normals[i - 2] = m.normals[i];
			}
		}

		void process_normals(Mesh& m, f32 smoothing_angle) {
			const f32 cos_alpha{ XMScalarCos(PI - smoothing_angle * PI / 180.f) };
			const bool is_hard_edge{ XMScalarNearEqual(smoothing_angle, 180.f, EPSILON) };
			const bool is_soft_edge{ XMScalarNearEqual(smoothing_angle, 0.f, EPSILON) };
			const u32 num_indicies{ (u32)m.raw_indicies.size() };
			const u32 num_verticies{ (u32)m.positions.size() };
			assert(num_indicies && num_verticies);

			m.indicies.resize(num_indicies);

			util::vector<util::vector<u32>> idx_ref(num_verticies);
			for (u32 i{ 0 }; i < num_indicies; ++i) {
				idx_ref[m.raw_indicies[i]].emplace_back(i);
			}

			for (u32 i{ 0 }; i < num_verticies; ++i) {
				auto& refs{ idx_ref[i] };
				u32 num_refs{ (u32)refs.size() };

				for (u32 j{ 0 }; j < num_refs; ++j) {
					m.indicies[refs[j]] = (u32)m.verticies.size();
					Vertex& v{ m.verticies.emplace_back() };
					v.position = m.positions[m.raw_indicies[refs[j]]];

					XMVECTOR n1{ XMLoadFloat3(&m.normals[refs[j]]) };
					if (!is_hard_edge) {
						for (u32 k{ j + 1 }; k < num_refs; ++k) {
							f32 cos_theta{ 0.f };
							XMVECTOR n2{ XMLoadFloat3(&m.normals[refs[k]]) };
							if (!is_soft_edge) {
								// cos(angle) = dot(n1, n2) / (|n1| * |n2|)
								XMStoreFloat(&cos_theta, XMVector3Dot(n1, n2) * XMVector3ReciprocalLength(n1));
							}

							if (is_soft_edge || cos_theta >= cos_alpha) {
								n1 += n2;
								m.indicies[refs[k]] = m.indicies[refs[j]];
								refs.erase(refs.begin() + k);
								--num_refs;
								--k;
							}
						}
					}
					XMStoreFloat3(&v.normal, XMVector3Normalize(n1));
				}
			}
		}

		void process_uvs(Mesh& m) {
			util::vector<Vertex> old_verticies;
			old_verticies.swap(m.verticies);
			util::vector<u32> old_indicies(m.indicies.size());
			old_indicies.swap(m.indicies);

			const u32 num_verticies{ (u32)old_verticies.size() };
			const u32 num_indicies{ (u32)old_indicies.size() };

			assert(num_verticies && num_indicies);

			util::vector<util::vector<u32>> idx_ref(num_verticies);
			for (u32 i{ 0 }; i < num_indicies; ++i) {
				idx_ref[old_indicies[i]].emplace_back(i);
			}
			for (u32 i{ 0 }; i < num_indicies; ++i) {
				auto& refs{ idx_ref[i] };
				u32 num_refs{ (u32)refs.size() };
				for (u32 j{ 0 }; j < num_refs; ++j) {
					m.indicies[refs[j]] = (u32)m.verticies.size();
					Vertex& v{ old_verticies[old_indicies[refs[j]]] };
					v.uv = m.uv_sets[0][refs[j]];
					m.verticies.emplace_back(v);

					for (u32 k{ j + 1 }; k < num_refs; ++k) {
						v2& uv1{ m.uv_sets[0][refs[k]] };
						if (XMScalarNearEqual(v.uv.x, uv1.x, EPSILON) && XMScalarNearEqual(v.uv.y, uv1.y, EPSILON)) {
							m.indicies[refs[k]] = m.indicies[refs[j]];
							refs.erase(refs.begin() + k);
							--num_refs;
							--k;
						}
					}
				}
			}
		}

		void pack_verticies_static(Mesh& m) {
			const u32 num_verticies{ (u32)m.verticies.size() };
			assert(num_verticies);
			m.packed_verticies_static.reserve(num_verticies);

			for (u32 i{ 0 }; i < num_verticies; ++i) {
				Vertex& v{ m.verticies[i] };
				const u8 signs{ (u8)((v.normal.z > 0.f) << 1) };
				const u16 normal_x{ (u16)pack_float<16>(v.normal.x, -1.f, 1.f) };
				const u16 normal_y{ (u16)pack_float<16>(v.normal.y, -1.f, 1.f) };

				m.packed_verticies_static.emplace_back(packed_vertex::VertexStatic{
					v.position,
					{ 0, 0, 0},
					signs,
					{ normal_x, normal_y },
					{},
					v.uv
				});
			}
		}

		void process_verticies(Mesh& m, const GeometryImportSettings& settings) {
			assert((m.raw_indicies.size() % 3) == 0);
			if (settings.calculate_normals || m.normals.empty()) {
				recalculate_normals(m);
			}
			process_normals(m, settings.smoothing_angle);

			if (!m.uv_sets.empty()) {
				process_uvs(m);
			}

			pack_verticies_static(m);
		}
		u64 get_mesh_size(const Mesh& m) {
			const u64 num_verticies{ m.verticies.size() };
			const u64 vertex_buffer_size(sizeof(packed_vertex::VertexStatic) * num_verticies);
			const u64 index_size{ (num_verticies < (1 << 16)) ? sizeof(u16) : sizeof(u32) };
			const u64 index_buffer_size{ index_size * m.indicies.size() };
			constexpr u64 su32{ sizeof(u32) };
			const u64 size{
				su32 +					// name length
				m.name.size() +			// mesh name string size
				su32 +					// lod id
				su32 +					// vertex size
				su32 +					// number of verticies
				su32 +					// index size (16 bit || 32 bit)
				su32 +					// number of indicies
				sizeof(f32) +			// LOD threshold
				vertex_buffer_size +	// room for verticies
				index_buffer_size		// room for indicies
			};
			return size;
		}

		u64 get_scene_size(const Scene& scene) {
			constexpr u64 su32{ sizeof(u32) };
			u64 size{
				su32 +					// scene name length
				scene.name.size() +		// scene name string size
				su32					// number of LODs
			};

			for (auto& lod : scene.lod_groups) {
				u64 lod_size{
					su32 +				// LOD name length
					lod.name.size() +	// LOD name string size
					su32				// number of mashes in this LOD
				};

				for (auto& m : lod.meshes) {
					lod_size += get_mesh_size(m);
				}
				size += lod_size;
			}
			return size;
		}

		void pack_mesh_data(const Mesh& m, u8* const buffer, u64& at) {
			constexpr u64 su32{ sizeof(u32) };
			u32 s{ 0 };

			s = (u32)m.name.size();
			memcpy(&buffer[at], &s, su32);								// write mesh name size
			at += su32;
			memcpy(&buffer[at], m.name.c_str(), s);						// write mesh name
			at += s;
			s = m.lod_id;
			memcpy(&buffer[at], &s, su32);								// write LOD id
			at += su32;

			constexpr u32 vertex_size{ sizeof(packed_vertex::VertexStatic) };
			s = vertex_size;
			memcpy(&buffer[at], &s, su32);								// write vertex size
			at += su32;
			const u32 num_verticies{ (u32)m.verticies.size() };
			s = num_verticies;
			memcpy(&buffer[at], &s, su32);								// write number of verticies
			at += su32;
			const u32 index_size{ (num_verticies < (1 << 16)) ? sizeof(u16) : sizeof(u32) };
			s = index_size;
			memcpy(&buffer[at], &s, su32);								// write index size (16 bit || 32 bit)
			at += su32;
			const u32 num_indicies{ (u32)m.indicies.size() };
			s = num_indicies;
			memcpy(&buffer[at], &s, su32);								// write number of indicies
			at += su32;
			memcpy(&buffer[at], &m.lod_treshold, sizeof(f32));			// wite LOD threshold
			at += sizeof(f32);
			s = vertex_size * num_verticies;
			memcpy(&buffer[at], m.packed_verticies_static.data(), s);	// write vertex data
			at += s;
			s = index_size * num_indicies;
			void* data{ (void*)m.indicies.data() };
			util::vector<u16> indicies;
			if (index_size == sizeof(u16)) {
				indicies.resize(num_indicies);
				for (u32 i{ 0 }; i < num_indicies; ++i) indicies[i] = (u16)m.indicies[i];
				data = (void*)indicies.data();
			}
			memcpy(&buffer[at], data, s);								// write index data
			at += s;
		}
	}

	void process_scene(Scene& scene, const GeometryImportSettings& settings) {
		for (auto& lod : scene.lod_groups) {
			for (auto& m : lod.meshes) {
				process_verticies(m, settings);
			}
		}
	}

	void pack_data(const Scene& scene, SceneData& data) {
		constexpr u64 su32{ sizeof(u32) };
		const u64 scene_size{ get_scene_size(scene) };
		data.buffer_size = (u32)scene_size;
		data.buffer = (u8*)CoTaskMemAlloc(scene_size);
		assert(data.buffer);

		u8* const buffer{ data.buffer };
		u64 at{ 0 };
		u32 s{ 0 };

		s = (u32)scene.name.size();
		memcpy(&buffer[at], &s, su32);					// write scene name length
		at += su32;
		memcpy(&buffer[at], scene.name.c_str(), s);		// write scene name
		at += s;
		s = (u32)scene.lod_groups.size();
		memcpy(&buffer[at], &s, su32);					// write number of LODs
		at += su32;

		for (auto& lod : scene.lod_groups) {
			s = (u32)lod.name.size();
			memcpy(&buffer[at], &s, su32);				// write LOD name size
			at += su32;
			memcpy(&buffer[at], lod.name.c_str(), s);	// write LOD name
			at += s;
			s = (u32)lod.meshes.size();
			memcpy(&buffer[at], &s, su32);				// write number of meshes in LOD
			at += su32;

			for (auto& m : lod.meshes) {
				pack_mesh_data(m, buffer, at);
			}
		}
	};
}