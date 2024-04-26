// Entity.h

#pragma once
#include "ComponentsCommonHeaders.h"

namespace lightning {
    #define INIT_INFO(component) namespace component { struct InitInfo; }
  
    INIT_INFO(transform);

    #undef INIT_INFO

    namespace game_entity {
        struct EntityInfo
        {
        transform::InitInfo* transform{ nullptr };
        };

        Entity create_game_entity(const EntityInfo& info);
        void remove_game_entity(Entity entity);
        bool is_alive(Entity entity);
    }
}
