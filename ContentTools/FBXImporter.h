#pragma once
#include "ToolsCommon.h"
#include <fbxsdk.h>

namespace lightning::tools {

	struct SceneData;
	struct Scene;
	struct Mesh;
	struct GeometryImportSettings;

	class FbxContext {
		public:
			FbxContext(const char* file, Scene* scene, SceneData* data) : _scene{ scene }, _scene_data{ data } {
				assert(file && _scene && _scene_data);
				if (initialize_fbx()) {
					load_fbx_file(file);
					assert(is_valid());
				}
			}

			~FbxContext() {
				_fbx_scene->Destroy();
				_fbx_manager->Destroy();
				ZeroMemory(this, sizeof(FbxContext));
			}

			void get_scene(FbxNode* root = nullptr);

			constexpr bool is_valid() const { return _fbx_manager && _fbx_scene; }
			constexpr f32 scene_scale() const { return _scene_scale; }

		private:

			bool initialize_fbx();
			void load_fbx_file(const char* file);
			void get_mesh(FbxNode* node, util::vector<Mesh>& meshes);
			void get_lod_group(FbxNode* node);

			Scene* _scene{ nullptr };
			SceneData* _scene_data{ nullptr };
			FbxManager* _fbx_manager{ nullptr };
			FbxScene* _fbx_scene{ nullptr };
			f32 _scene_scale{ 1.f };
	};

}