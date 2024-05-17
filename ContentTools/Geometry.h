#pragma once
#include "ToolsCommon.h"

namespace lightning::tools {

	struct Vertex {
		math::v4 tangent{};
		math::v3 position{};
		math::v3 normal{};
		math::v2 uv{};
	};

	struct Mesh {
		util::vector<math::v3> positions;
		util::vector<math::v3> normals;
		util::vector<math::v4> tangents;
		util::vector<util::vector<math::v2>> uv_sets;
		util::vector<u32> raw_indicies;

		util::vector<Vertex> verticies;
		util::vector<u32> indicies;
	};

	struct LodGroup {
		std::string name;
		util::vector<Mesh> meshes;
	};

	struct Scene {
		std::string name;
		util::vector<LodGroup> lod_groups;
	};

	struct GeometryImportSettings {
		f32 smoothing_angle;
		u8 calculate_normals;
		u8 calculatetangents;
		u8 reverse_handedness;
		u8 import_embeded_textures;
		u8 import_animations;
	};

	struct SceneData {
		u8* buffer;
		u32 buffer_size;
		GeometryImportSettings settings;
	};

	void process_scene(Scene& scene, const GeometryImportSettings& settings);
	void pack_data(const Scene& scene, SceneData& data);
}