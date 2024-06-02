#include "FBXImporter.h"
#include "Geometry.h"

#if _DEBUG
#pragma comment (lib, "../packages/FBX SDK/lib/x64/debug/libfbxsdk-md.lib")
#pragma comment (lib, "../packages/FBX SDK/lib/x64/debug/libxml2-md.lib")
#pragma comment (lib, "../packages/FBX SDK/lib/x64/debug/zlib-md.lib")
#else
#pragma comment (lib, "../packages/FBX SDK/x64/release/libfbxsdk-md.lib")
#pragma comment (lib, "../packages/FBX SDK/x64/release/libxml2-md.lib")
#pragma comment (lib, "../packages/FBX SDK/x64/release/zlib-md.lib")
#endif

namespace lightning::tools {
	namespace {
	
		std::mutex fbx_mutex{};

	}

	bool FbxContext::initialize_fbx() {
		assert(!is_valid());

		_fbx_manager = FbxManager::Create();
		if (!_fbx_manager) return false;

		FbxIOSettings* ios{ FbxIOSettings::Create(_fbx_manager, IOSROOT) };
		assert(ios);
		_fbx_manager->SetIOSettings(ios);

		return true;
	}

	void FbxContext::load_fbx_file(const char* file) {
		assert(_fbx_manager && !_fbx_scene);
		_fbx_scene = FbxScene::Create(_fbx_manager, "Importer Scene");
		if (!_fbx_scene) return;
		FbxImporter* importer{ FbxImporter::Create(_fbx_manager, "Importer") };
		if (!(importer && importer->Initialize(file, -1, _fbx_manager->GetIOSettings()) && importer->Import(_fbx_scene))) return;
		importer->Destroy();
		_scene_scale = (f32)_fbx_scene->GetGlobalSettings().GetSystemUnit().GetConversionFactorTo(FbxSystemUnit::m);
	}

	void FbxContext::get_scene(FbxNode* root) {
		assert(is_valid());
		if (!root) {
			root = _fbx_scene->GetRootNode();
			if (!root) return;
		}

		const s32 num_nodes{ root->GetChildCount() };
		for (s32 i{ 0 }; i < num_nodes; ++i) {
			FbxNode* node{ root->GetChild(i) };
			if (!node) continue;

			if (node->GetMesh()) {
				LodGroup lod{};
				get_mesh(node, lod.meshes);
				if (lod.meshes.size()) {
					lod.name = lod.meshes[0].name;
					_scene->lod_groups.emplace_back(lod);
				}
			}
			else if (node->GetLodGroup()) {
				get_lod_group(node);
			}
			else {
				get_scene(node);
			}
		}
	}

	void FbxContext::get_mesh(FbxNode* node, util::vector<Mesh>& meshes) {
		assert(node);

		if (FbxMesh * fbx_mesh{ node->GetMesh() }) {
			if (fbx_mesh->RemoveBadPolygons() < 0) return;

			FbxGeometryConverter gc{ _fbx_manager };
			fbx_mesh = static_cast<FbxMesh*>(gc.Triangulate(fbx_mesh, true));
			if (!fbx_mesh || fbx_mesh->RemoveBadPolygons() < 0) return;

			Mesh m;
			m.lod_id = (u32)meshes.size();
			m.lod_treshold = -1.f;
			m.name = (node->GetName()[0] != '\0') ? node->GetName() : fbx_mesh->GetName();
			if (get_mesh_data(fbx_mesh, m)) {
				meshes.emplace_back(m);
			}
		}
	}

	void FbxContext::get_lod_group(FbxNode* node) {

	}

	EDITOR_INTERFACE void import_fbx(const char* file, SceneData* data) {
		assert(file && data);
		Scene scene{};

		{
			std::lock_guard lock{ fbx_mutex };

			FbxContext fbx_context{ file, &scene, data };
			if (fbx_context.is_valid()) {

			}
			else {
				return;
			}
		}

		process_scene(scene, data->settings);
		pack_data(scene, *data);
	}
}