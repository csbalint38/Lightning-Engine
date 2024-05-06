#include "Entity.h"
#include "Transform.h"
#include "Script.h"

namespace lightning::game_entity {

    namespace {

        util::vector<id::generation_type> generations;
        util::deque<entity_id> free_ids;
       
        util::vector<transform::Component> transforms;
        util::vector<script::Component> scripts;
    }

    Entity create(EntityInfo info) {
        assert(info.transform);
        if (!info.transform) return Entity{};

        entity_id id;

        if(free_ids.size() > id::min_deleted_elements) {
            id = free_ids.front();
            assert(!is_alive(id));
            free_ids.pop_front();
            id = entity_id{ id::new_generation(id) };
            ++generations[id::index(id)];
        }
        else {
            id = entity_id{ (id::id_type)generations.size() };
            generations.push_back(0);

            transforms.emplace_back();
            scripts.emplace_back();
        }

        const Entity new_entity{ id };
        const id::id_type index{ id::index(id) };

        // Create transform component
        assert(!transforms[index].is_valid());
        transforms[index] = transform::create(*info.transform, new_entity);
        if (!transforms[index].is_valid()) return {};

        // Create script component
        if (info.script && info.script->script_creator) {
            assert(!scripts[index].is_valid());
            scripts[index] = script::create(*info.script, new_entity);
            assert(scripts[index].is_valid());
        }

        return new_entity;
    }
    void remove(entity_id  id) {
        const id::id_type index{ id::index(id) };
        assert(is_alive(id));

        if (scripts[index].is_valid()) {
            script::remove(scripts[index]);
            scripts[index] = {};
        }

        transform::remove(transforms[index]);
        transforms[index] = {};
        free_ids.push_back(id);
    }
    bool is_alive(entity_id id) {
        assert(id::is_valid(id));
        const id::id_type index{ id::index(id) };
        assert(index < generations.size());
        assert(generations[index] == id::generation(id));
        return (generations[index] == id::generation(id) && transforms[index].is_valid());
    }

    transform::Component Entity::transform() const {
        assert(is_alive(_id));
        const id::id_type index{ id::index(_id) };
        return transforms[index];
    }

    script::Component Entity::script() const {
        assert(is_alive(_id));
        const id::id_type index{ id::index(_id) };
        return scripts[index];
    }
}

