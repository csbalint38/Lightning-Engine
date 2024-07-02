#include "CommonHeaders.h"
#include "Content/ContentToEngine.h"
#include "Graphics/Renderer.h"
#include "ShaderCompilation.h"
#include "Components/Entity.h"
#include "../ContentTools/Geometry.h"

#include <filesystem>

#undef OPAQUE

using namespace lightning;

game_entity::Entity create_one_game_entity(math::v3 position, math::v3 rotation, const char* script_name);
void remove_game_entity(game_entity::entity_id id);

bool read_file(std::filesystem::path, std::unique_ptr<u8[]>&, u64&);

namespace {
	id::id_type building_model_id{ id::invalid_id };
	id::id_type fan_model_id{ id::invalid_id };
	id::id_type blades_model_id{ id::invalid_id };

	id::id_type building_item_id{ id::invalid_id };
	id::id_type fan_item_id{ id::invalid_id };
	id::id_type blades_item_id{ id::invalid_id };

	game_entity::entity_id building_entity_id{ id::invalid_id };
	game_entity::entity_id fan_entity_id{ id::invalid_id };
	game_entity::entity_id blades_entity_id{ id::invalid_id };

	id::id_type vs_id{ id::invalid_id };
	id::id_type ps_id{ id::invalid_id };
	id::id_type material_id{ id::invalid_id };

	std::unordered_map<id::id_type, game_entity::entity_id> render_item_entity_map;

	[[nodiscard]] id::id_type load_model(const char* path) {
		std::unique_ptr<u8[]> model;
		u64 size{ 0 };
		read_file(path, model, size);

		const id::id_type model_id{ content::create_resource(model.get(), content::AssetType::MESH) };
		assert(id::is_valid(model_id));

		return model_id;
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
	assert(id::is_valid(vs_id) && id::is_valid(ps_id));
	graphics::MaterialInitInfo info{};
	info.shader_ids[graphics::ShaderType::VERTEX] = vs_id;
	info.shader_ids[graphics::ShaderType::PIXEL] = ps_id;
	info.type = graphics::MaterialType::OPAQUE;

	material_id = content::create_resource(&info, content::AssetType::MATERIAL);
}

void remove_item(id::id_type item_id, id::id_type model_id) {
	if (id::is_valid(item_id)) {
		graphics::remove_render_item(item_id);
		auto pair = render_item_entity_map.find(item_id);
		if (pair != render_item_entity_map.end()) {
			remove_game_entity(pair->second);
		}

		if (id::is_valid(model_id)) {
			content::destroy_resource(model_id, content::AssetType::MESH);
		}
	}
}

void create_render_items() {
	auto _1 = std::thread{ [] { building_model_id = load_model("C:/Users/balin/Documents/Lightning-Engine/EngineTest/villa.model"); }};
	auto _2 = std::thread{ [] { fan_model_id = load_model("C:/Users/balin/Documents/Lightning-Engine/EngineTest/turbine.model"); }};
	auto _3 = std::thread{ [] { blades_model_id = load_model("C:/Users/balin/Documents/Lightning-Engine/EngineTest/blades.model"); }};
	auto _4 = std::thread{ [] { load_shaders(); } };

	building_entity_id = create_one_game_entity({0, 0, 0}, {}, nullptr).get_id();
	fan_entity_id = create_one_game_entity({0, 0, 69.78f}, {}, nullptr).get_id();
	blades_entity_id = create_one_game_entity({ -.152f, 60.555f, 66.362 }, {}, "TurbineScript").get_id();

	_1.join();
	_2.join();
	_3.join();
	_4.join();

	create_material();
	id::id_type materials[]{ material_id };

	building_item_id = graphics::add_render_item(building_entity_id, building_model_id, _countof(materials), &materials[0]);
	fan_item_id = graphics::add_render_item(fan_entity_id, fan_model_id, _countof(materials), &materials[0]);
	blades_item_id = graphics::add_render_item(blades_entity_id, blades_model_id, _countof(materials), &materials[0]);

	render_item_entity_map[building_item_id] = building_entity_id;
	render_item_entity_map[fan_item_id] = fan_entity_id;
	render_item_entity_map[blades_item_id] = blades_entity_id;
}

void destroy_render_items() {

	remove_item(building_item_id, building_model_id);
	remove_item(fan_item_id, fan_model_id);
	remove_item(blades_item_id, blades_model_id);

	if (id::is_valid(material_id)) content::destroy_resource(material_id, content::AssetType::MATERIAL);
	if (id::is_valid(vs_id)) content::remove_shader_group(vs_id);
	if (id::is_valid(ps_id)) content::remove_shader_group(ps_id);
}

void get_render_items(id::id_type* items, [[maybe_unused]] u32 count) {
	assert(count == 3);
	items[0] = building_item_id;
	items[1] = fan_item_id;
	items[2] = blades_item_id;
}