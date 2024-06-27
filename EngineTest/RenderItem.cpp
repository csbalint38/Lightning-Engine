#include "CommonHeaders.h"
#include "Content/ContentToEngine.h"
#include "Graphics/Renderer.h"
#include "ShaderCompilation.h"
#include "Components/Entity.h"
#include "../ContentTools/Geometry.h"

#include <filesystem>

#undef OPAQUE

using namespace lightning;

bool read_file(std::filesystem::path, std::unique_ptr<u8[]>&, u64&);

namespace {
	id::id_type model_id{ id::invalid_id };
	id::id_type vs_id{ id::invalid_id };
	id::id_type ps_id{ id::invalid_id };
	id::id_type material_id{ id::invalid_id };

	std::unordered_map<id::id_type, id::id_type> render_item_entity_map;

	void load_model() {
		std::unique_ptr<u8[]> model;
		u64 size{ 0 };
		read_file("..\\..\\EngineTest\\robot_model.model", model, size);

		model_id = content::create_resource(model.get(), content::AssetType::MESH);
		assert(id::is_valid(model_id));
	}

	void load_shaders() {
		ShaderFileInfo info{};
		info.file_name = "TestShader.hlsl";
		info.function = "test_shader_vs";
		info.type = ShaderType::VERTEX;

		const char* shader_path{ "../../EngineTest/" };

		std::wstring defines[]{ L"ELEMENTS_TYPE=1", L"ELEMENTS_TYPE=3" };
		util::vector<u32> keys;
		keys.emplace_back(tools::elements::ElementsType::STATIC_NORMAL);
		keys.emplace_back(tools::elements::ElementsType::STATIC_NORMAL_TEXTURE);

		util::vector<std::wstring> extra_args{};
		util::vector<std::unique_ptr<u8[]>> vertex_shaders;
		util::vector<const u8*> vertex_shader_pointers;

		for (u32 i{ 0 }; i < _countof(defines); ++i) {
			extra_args.clear();
			extra_args.emplace_back(L"-D");
			extra_args.emplace_back(defines[i]);

			vertex_shaders.emplace_back(std::move(compile_shader(info, shader_path, extra_args)));
			assert(vertex_shaders.back().get());

			vertex_shader_pointers.emplace_back(vertex_shaders.back().get());
		}

		info.function = "test_shader_ps";
		info.type = ShaderType::PIXEL;

		auto pixel_shader = compile_shader(info, shader_path, extra_args);
		assert(pixel_shader.get());

		vs_id = content::add_shader_group(vertex_shader_pointers.data(), vertex_shader_pointers.size(), keys.data());

		const u8* pixel_shaders[]{ pixel_shader.get() };
		ps_id = content::add_shader_group(&pixel_shaders[0], 1, &u32_invalid_id);
	}
}

void create_material() {
	graphics::MaterialInitInfo info{};
	info.shader_ids[graphics::ShaderType::VERTEX] = vs_id;
	info.shader_ids[graphics::ShaderType::PIXEL] = ps_id;
	info.type = graphics::MaterialType::OPAQUE;

	material_id = content::create_resource(&info, content::AssetType::MATERIAL);
}

id::id_type create_render_item(id::id_type entity_id) {
	auto _1 = std::thread{ [] { load_model(); } };
	auto _2 = std::thread{ [] { load_shaders(); } };

	_1.join();
	_2.join();

	create_material();
	id::id_type materials[]{ material_id, material_id, material_id, material_id, material_id };

	id::id_type item_id{ graphics::add_render_item(entity_id, model_id, _countof(materials), &materials[0]) };

	render_item_entity_map[item_id] = entity_id;

	return item_id;
}

void destroy_render_item(id::id_type item_id) {
	if (id::is_valid(item_id)) {

		graphics::remove_render_item(item_id);
		auto pair = render_item_entity_map.find(item_id);
		if (pair != render_item_entity_map.end()) {
			game_entity::remove(game_entity::entity_id{ pair->second });
		}
	}

	if (id::is_valid(material_id)) content::destroy_resource(material_id, content::AssetType::MATERIAL);
	if (id::is_valid(vs_id)) content::remove_shader_group(vs_id);
	if (id::is_valid(ps_id)) content::remove_shader_group(ps_id);
	if (id::is_valid(model_id)) content::destroy_resource(model_id, content::AssetType::MESH);
}