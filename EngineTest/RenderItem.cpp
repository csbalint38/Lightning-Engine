#include "CommonHeaders.h"
#include "Content/ContentToEngine.h"
#include "ShaderCompilation.h"

#include <filesystem>

using namespace lightning;

bool read_file(std::filesystem::path, std::unique_ptr<u8[]>&, u64&);

namespace {
	id::id_type model_id{ id::invalid_id };
	id::id_type vs_id{ id::invalid_id };
	id::id_type ps_id{ id::invalid_id };

	void load_model() {
		std::unique_ptr<u8[]> model;
		u64 size{ 0 };
		read_file("..\\..\\EngineTest\\model.model", model, size);

		model_id = content::create_resource(model.get(), content::AssetType::MESH);
		assert(id::is_valid(model_id));
	}

	void load_shaders() {

	}
}

id::id_type create_render_item(id::id_type entity_id) {
	auto _1 = std::thread{ [] { load_model(); } };
	auto _2 = std::thread{ [] { load_shaders(); } };

	_1.join();
	_2.join();

	return id::invalid_id;
}

void destroy_render_item(id::id_type item_id) {

}