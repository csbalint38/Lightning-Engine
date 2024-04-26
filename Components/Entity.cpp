#include "Entity.h"

namespace lightning::game_entity {

  namespace {

    util::vector<id::generation_type> generations;
    util::deque<entity_id> free_ids;

    Entity create_game_entity(const EntityInfo& info) {
      assert(info.transform);
      if (!info.transform) return Entity{};

      entity_id id;

      if(free_ids.size() > id::min_deleted_elements) {
        id = free_ids.front();
        assert(!is_alive(Entity{ id }));
        free_ids.pop_front();
        id = entity_id{ id::new_generation(id) };
        ++generations[id::index(id)];
      }
      else {
        id = entity_id{ (id::id_type)generations.size() };
        generations.push_back(0);
      }

      const Entity new_entity{ id };
      const id::id_type index{ id::indec(id) };

      return new_entity;
    }
    void remove_game_entity(Entity entity) {
      cosnt entity_id id{ entity.get_id() };
      const id::id_type index{ id::index(id) };
      assert(is_alive(entity));
      if (is_alive(entity)) {
        free_ids.push_back(id);
      }
    }
    bool is_alive(Entity entity) {
      const entity_id id{ entity.get_id() };
      const id::id_type index{ id::index{id} };
      assert(index < generations.size());
      assert(generations[index] == id::generation(id));
      return (generations[index] == id::generation(id));
    }
  }
}
