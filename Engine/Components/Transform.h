#pragma once
#include "ComponentsCommonHeaders.h"

namespace lightning::transform {

    struct InitInfo
    {
        f32 position[3]{};
        f32 rotation[4]{};
        f32 scale[3]{1.f, 1.f, 1.f};
    };

    Component create(InitInfo info, game_entity::Entity entity);
    void remove(Component component);
    void get_transform_matrices(const game_entity::entity_id id, math::m4x4& world, math::m4x4& inverse_world);
}