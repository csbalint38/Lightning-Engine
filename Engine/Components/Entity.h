// Entity.h

#pragma once
#include "ComponentsCommonHeaders.h"

namespace lightning {
    #define INIT_INFO(component) namespace component { struct InitInfo; }
  
    INIT_INFO(transform);
    INIT_INFO(script);

    #undef INIT_INFO

    namespace game_entity {
        struct EntityInfo
        {
        transform::InitInfo* transform{ nullptr };
        script::InitInfo* script{ nullptr };
        };

        Entity create(const EntityInfo& info);
        void remove(entity_id id);
        bool is_alive(entity_id id);
    }
}
