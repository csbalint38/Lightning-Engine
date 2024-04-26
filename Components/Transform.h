#pragma once
#include "ComponentsCommonHeaders.h"

namespace lightning::transform {

    struct InitInfo
    {
        f32 position[3]{};
        f32 rotation[4]{};
        f32 scale[3]{1.f, 1.f, 1.f};
    };

    Component create_transform(const InitInfo& info, game_entity::Entity entity);
    void remove_transform(Component component);
}