#include "CommonHeaders.h"
#include "Content/ContentToEngine.h"
#include "ShaderCompilation.h"
#include "Components/Entity.h"

#include <filesystem>

using namespace lightning;

bool read_file(std::filesystem::path, std::unique_ptr<u8[]>&, u64&);

namespace {
	id::id_type model_id{ id::invalid_id };
	id::id_type vs_id{ id::invalid_id };
	id::id_type ps_id{ id::invalid_id };

	std::unordered_map<id::id_type, id::id_type> render_item_entity_map;

	void load_model() {
		std::unique_ptr<u8[]> model;
		u64 size{ 0 };
		read_file("..\\..\\EngineTest\\model.model", model, size);

		model_id = content::create_resource(model.get(), content::AssetType::MESH);
		assert(id::is_valid(model_id));
	}

	void load_shaders() {
		ShaderFileInfo info{};
		info.file_name = "TestShader.hlsl";
		info.function = "test_shader_vs";
		info.type = ShaderType::VERTEX;

		const char* shader_path{ "../../EngineTest/" };

		auto vertex_shader = compile_shader(info, shader_path);
		assert(vertex_shader.get());

		info.function = "test_shader_ps";
		info.type = ShaderType::PIXEL;

		auto pixel_shader = compile_shader(info, shader_path);
		assert(pixel_shader.get());

		vs_id = content::add_shader(vertex_shader.get());
		ps_id = content::add_shader(pixel_shader.get());
	}
}

id::id_type create_render_item(id::id_type entity_id) {
	auto _1 = std::thread{ [] { load_model(); } };
	auto _2 = std::thread{ [] { load_shaders(); } };

	_1.join();
	_2.join();

	id::id_type item_id{ 0 };

	render_item_entity_map[item_id] = entity_id;

	return item_id;
}

void destroy_render_item(id::id_type item_id) {
	if (id::is_valid(item_id)) {
		auto pair = render_item_entity_map.find(item_id);
		if (pair != render_item_entity_map.end()) {
			game_entity::remove(game_entity::entity_id{ pair->second });
		}
	}

	if (id::is_valid(vs_id)) content::remove_shader(vs_id);
	if (id::is_valid(ps_id)) content::remove_shader(ps_id);
	if (id::is_valid(model_id)) content::destroy_resource(model_id, content::AssetType::MESH);
}