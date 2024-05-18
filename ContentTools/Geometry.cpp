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
	}

	void process_scene(Scene& scene, const GeometryImportSettings& settings) {
		for (auto& lod : scene.lod_groups) {
			for (auto& m : lod.meshes) {
				process_verticies(m, settings);
			}
		}
	}

	void pack_data(const Scene& scene, SceneData& data) {};
}