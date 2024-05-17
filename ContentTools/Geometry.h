#pragma once
#include "ToolsCommon.h"

namespace lightning::tools {

	struct Mesh {
		util::vector<math::v3> positions;
		util::vector<math::v3> normals;
		util::vector<math::v4> tangents;
		util::vector<util::vector<math::v2>> uv_sets;
		util::vector<u32> raw_indicies;
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
}