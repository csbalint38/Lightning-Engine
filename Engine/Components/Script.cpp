#include "Entity.h"
#include "Script.h"

namespace lightning::script {
	namespace {
		util::vector<id::generation_type> generations;
		util::deque<script_id> free_ids;

		util::vector<detail::script_ptr> entity_scripts;
		util::vector<id::id_type> id_mapping;

		bool exists(script_id id) {
			assert(id::is_valid(id));
			const id::id_type index{ id::index(id) };
			assert(index < generations.size() && id_mapping[index] < entity_scripts.size());
			assert(generations[index] == id::generation(id));
			return (generations[index] == id::generation(id)) && entity_scripts[id_mapping[index]] && entity_scripts[id_mapping[index]]->is_valid();
		}
	}

	Component create(InitInfo info, game_entity::Entity entity) {
		assert(entity.is_valid());
		assert(info.script_creator);

		script_id id{};

		if (free_ids.size() > id::min_deleted_elements) {
			id = free_ids.front();
			assert(!exists(id));
			free_ids.pop_back();
			id = script_id{ id::new_generation(id) };
			++generations[id::index(id)];
		}
		else {
			id = script_id{ (id::id_type)id_mapping.size() };
			id_mapping.emplace_back();
			generations.push_back(0);
		}

		return Component{};
	}

	void remove(Component component) {

	}
}