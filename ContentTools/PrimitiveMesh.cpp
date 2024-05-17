#include "PrimitiveMesh.h"
#include "Geometry.h"

namespace lightning::tools {
	namespace {
		using namespace math;
		using primitive_mesh_creator = void(*)(Scene&, const PrimitiveInitInfo& info);

		void create_plane(Scene& scene, const PrimitiveInitInfo& info);
		void create_cube(Scene& scene, const PrimitiveInitInfo& info);
		void create_uv_sphere(Scene& scene, const PrimitiveInitInfo& info);
		void create_ico_sphere(Scene& scene, const PrimitiveInitInfo& info);
		void create_cylinder(Scene& scene, const PrimitiveInitInfo& info);
		void create_capsule(Scene& scene, const PrimitiveInitInfo& info);

		primitive_mesh_creator creators[]{
			create_plane,
			create_cube,
			create_uv_sphere,
			create_ico_sphere,
			create_cylinder,
			create_capsule
		};

		static_assert(_countof(creators) == PrimitiveMeshType::count);

		struct Axis {
			enum : u32 {
				x = 0,
				y = 1,
				z = 2
			};
		};

		Mesh create_plane(const PrimitiveInitInfo& info, u32 horizontal_index = Axis::x, u32 vertical_index = Axis::z, bool flip_winding = false, v3 offset = { -.5f, 0.f, -.5f }, v2 u_range = { 0.f, 1.f }, v2 v_range = { 0.f, 1.f }) {
			assert(horizontal_index < 3 && vertical_index < 3);
			assert(horizontal_index != vertical_index);

			const u32 horizontal_count{ clamp(info.segments[horizontal_index], 1u, 10u) };
			const u32 vertical_count{ clamp(info.segments[vertical_index], 1u, 10u) };
			const f32 horizontal_step{ 1.f / horizontal_count };
			const f32 vertical_step{ 1.f / vertical_count };
			const f32 u_step{ (u_range.y - u_range.x) / horizontal_count };
			const f32 v_step{ (v_range.y - v_range.x) / vertical_count };

			Mesh m{};
			util::vector<v2> uvs;

			for (u32 i{ 0 }; i <= vertical_count; ++i) {
				for (u32 j{ 0 }; j <= horizontal_count; ++j) {
					v3 position{ offset };
					f32* const as_array{ &position.x };
					as_array[horizontal_index] += j * horizontal_step;
					as_array[vertical_index] += i * vertical_step;
					m.positions.emplace_back(position.x * info.size.x, position.y * info.size.y, position.z * info.size.z);

					v2 uv{ u_range.x, 1.f - v_range.x };
					uv.x += j * u_step;
					uv.y += i * v_step;
					uvs.emplace_back(uv);
				}
			}

			assert(m.positions.size() == (((u64)horizontal_count + 1) * ((u64)vertical_count + 1)));

			const u32 row_length{ horizontal_count + 1 };

			for (u32 i{ 0 }; i < vertical_count; ++i) {
				u32 k{ 0 };
				for (u32 j{ k }; j < horizontal_count; ++j) {
					const u32 index[4]{
						j + i * row_length,
						j + (i + 1) * row_length,
						(j + 1) + i * row_length,
						(j + 1) + (i + 1) * row_length
					};
					m.raw_indicies.emplace_back(index[0]);
					m.raw_indicies.emplace_back(index[flip_winding ? 2 : 1]);
					m.raw_indicies.emplace_back(index[flip_winding ? 1 : 2]);

					m.raw_indicies.emplace_back(index[2]);
					m.raw_indicies.emplace_back(index[flip_winding ? 3 : 1]);
					m.raw_indicies.emplace_back(index[flip_winding ? 1 : 3]);
				}
				++k;
			}

			const u32 num_indicies{ 3 * 2 * horizontal_count * vertical_count };
			assert(m.raw_indicies.size() == num_indicies);

			for (u32 i{ 0 }; i < num_indicies; ++i) {
				m.uv_sets[0].emplace_back(uvs[m.raw_indicies[i]]);
			}

			return m;
		}

		void create_plane(Scene& scene, const PrimitiveInitInfo& info) {
			LodGroup lod{};
			lod.name = "plane";
			lod.meshes.emplace_back(create_plane(info));
			scene.lod_groups.emplace_back(lod);
		};

		void create_cube(Scene& scene, const PrimitiveInitInfo& info) {};
		void create_uv_sphere(Scene& scene, const PrimitiveInitInfo& info) {};
		void create_ico_sphere(Scene& scene, const PrimitiveInitInfo& info) {};
		void create_cylinder(Scene& scene, const PrimitiveInitInfo& info) {};
		void create_capsule(Scene& scene, const PrimitiveInitInfo& info) {};
	}

	EDITOR_INTERFACE void create_primitive_mesh(SceneData* data, PrimitiveInitInfo* info) {
		assert(data && info);
		assert(info->type < PrimitiveMeshType::count);
		Scene scene{};
		creators[info->type](scene, *info);

		data->settings.calculate_normals = 1;
		process_scene(scene, data->settings);
		pack_data(scene, *data);
	}
}
