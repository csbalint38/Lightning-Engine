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

    entity_id create_game_entity(const EntityInfo& info);
    void remove_game_entity(entity_id id);
    bool is_alive(entity_id id);
  }
}
