// Entity.h

#pragma once
#include "ComponentsCommonHeaders.h"

namespace lightning {

    struct ComponentType {
        enum Type : u32 {
            TRANSFORM,
            SCRIPT,
            GEOMETRY,

            count
        };
    };

    #define INIT_INFO(component) namespace component { struct InitInfo; }
  
    INIT_INFO(transform);
    INIT_INFO(script);
    INIT_INFO(geometry);

    #undef INIT_INFO

    namespace game_entity {
        struct EntityInfo
        {
        transform::InitInfo* transform{ nullptr };
        script::InitInfo* script{ nullptr };
        geometry::InitInfo* geometry{ nullptr };
        };

        Entity create(EntityInfo info);
        bool update_component(entity_id id, EntityInfo info, ComponentType::Type type);
        void remove(entity_id id);
        bool is_alive(entity_id id);
    }
}
