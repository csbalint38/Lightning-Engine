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
			m.lod_threshold = -1.f;
			m.name = (node->GetName()[0] != '\0') ? node->GetName() : fbx_mesh->GetName();
			if (get_mesh_data(fbx_mesh, m)) {
				meshes.emplace_back(m);
			}
		}
	}

	void FbxContext::get_lod_group(FbxNode* node) {
		assert(node);

		if (FbxLODGroup * lod_grp{ node->GetLodGroup() }) {
			LodGroup lod{};
			lod.name = (node->GetName()[0] != '\0' ? node->GetName() : lod_grp->GetName());

			const s32 num_lods{ lod_grp->GetNumThresholds() };
			const s32 num_nodes{ node->GetChildCount() };
			assert(num_lods > 0 && num_nodes > 0);

			for (s32 i{ 0 }; i < num_nodes; ++i) {
				get_mesh(node->GetChild(i), lod.meshes);

				if (lod.meshes.size() > 1 && lod.meshes.size() < num_lods + 1 && lod.meshes.back().lod_threshold < 0.f) {
					FbxDistance threshold;
					lod_grp->GetThreshold((u32)lod.meshes.size() - 2, threshold);
					lod.meshes.back().lod_threshold = threshold.value() * _scene_scale;
				}
			}

			if (lod.meshes.size()) _scene->lod_groups.emplace_back(lod);
		}
	}

	bool FbxContext::get_mesh_data(FbxMesh* fbx_mesh, Mesh& m) {
		assert(fbx_mesh);

		const s32 num_polys{ fbx_mesh->GetPolygonCount() };
		if (num_polys <= 0) return false;

		const s32 num_verticies{ fbx_mesh->GetControlPointsCount() };
		FbxVector4* verticies{ fbx_mesh->GetControlPoints() };
		const s32 num_indicies{ fbx_mesh->GetPolygonVertexCount() };
		s32* indices{ fbx_mesh->GetPolygonVertices() };

		assert(num_verticies > 0 && verticies && num_indicies > 0 && indices);
		if (!(num_verticies > 0 && verticies && num_indicies > 0 && indices)) return false;

		m.raw_indicies.resize(num_indicies);
		util::vector<u32> vertex_ref(num_verticies, u32_invalid_id);

		for (s32 i{ 0 }; i < num_indicies; ++i) {
			const u32 v_idx{ (u32)indices[i] };
			if (vertex_ref[v_idx] != u32_invalid_id) {
				m.raw_indicies[i] = vertex_ref[v_idx];
			}
			else {
				FbxVector4 v = verticies[v_idx] * _scene_scale;
				m.raw_indicies[i] = (u32)m.positions.size();
				vertex_ref[v_idx] = m.raw_indicies[i];
				m.positions.emplace_back((f32)v[0], (f32)v[1], (f32)v[2]);
			}
		}

		assert(m.raw_indicies.size() % 3 == 0);
		assert(num_polys > 0);
		FbxLayerElementArrayTemplate<s32>* mtl_indicies;
		if (fbx_mesh->GetMaterialIndices(&mtl_indicies)) {
			for (s32 i{ 0 }; i < num_polys; ++i) {
				const s32 mtl_index{ mtl_indicies->GetAt(i) };
				assert(mtl_index >= 0);
				m.material_indicies.emplace_back((u32)mtl_index);
				if (std::find(m.material_used.begin(), m.material_used.end(), (u32)mtl_index) == m.material_used.end()) {
					m.material_used.emplace_back((u32)mtl_index);
				}
			}
		}

		const bool import_normals{ !_scene_data->settings.calculate_normals };
		const bool import_tangents{ !_scene_data->settings.calculate_tangents };

		if (import_normals) {
			FbxArray<FbxVector4> normals;

			if (fbx_mesh->GenerateNormals() && fbx_mesh->GetPolygonVertexNormals(normals) && normals.Size() > 0) {
				const s32 num_normals{ normals.Size() };
				for (s32 i{ 0 }; i < num_normals; ++i) {
					m.normals.emplace_back((f32)normals[i][0], (f32)normals[i][1], (f32)normals[i][2]);
				}
			}
			else {
				_scene_data->settings.calculate_normals = true;
			}
		}

		if (import_tangents) {
			FbxLayerElementArrayTemplate<FbxVector4>* tangents{ nullptr };

			if (fbx_mesh->GenerateTangentsData() && fbx_mesh->GetTangents(&tangents) && tangents && tangents->GetCount() > 0) {
				const s32 num_tangents{ tangents->GetCount() };
				for (s32 i{ 0 }; i < num_tangents; ++i) {
					FbxVector4 t{ tangents->GetAt(i) };
					m.tangents.emplace_back((f32)t[0], (f32)t[1], (f32)t[2], (f32)t[3]);
				}
			}
			else {
				_scene_data->settings.calculate_tangents = true;
			}
		}

		FbxStringList uv_names;
		fbx_mesh->GetUVSetNames(uv_names);
		const s32 uv_set_count{ uv_names.GetCount() };

		m.uv_sets.resize(uv_set_count);

		for (s32 i{ 0 }; i < uv_set_count; ++i) {
			FbxArray<FbxVector2> uvs;
			if (fbx_mesh->GetPolygonVertexUVs(uv_names.GetStringAt(i), uvs)) {
				const s32 num_uvs{ uvs.Size() };
				for (s32 j{ 0 }; j < num_uvs; ++j) {
					m.uv_sets[i].emplace_back((f32)uvs[j][0], (f32)uvs[j][1]);
				}
			}
		}

		return true;
	}


	EDITOR_INTERFACE void import_fbx(const char* file, SceneData* data) {
		assert(file && data);
		Scene scene{};

		{
			std::lock_guard lock{ fbx_mutex };

			FbxContext fbx_context{ file, &scene, data };
			if (fbx_context.is_valid()) {
				fbx_context.get_scene();
			}
			else {
				return;
			}
		}

		process_scene(scene, data->settings);
		pack_data(scene, *data);
	}
}