// Entity.h

#pragma once
#include "ComponentsCommonHeaders.h"

namespace lightning {

  struct EntityInfo
  {
    /* data */
  };

  u32 create_game_entity(const EntityInfo& info);
  void remove_game_entity(u32 id);
}
