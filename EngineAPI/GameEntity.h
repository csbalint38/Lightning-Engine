#pragma once

#include "..\Components\ComponentsCommonHeaders.h"
#include "TransformComponent.h"

namespace lightning::game_entity {
	DEFINE_TYPED_ID(entity_id);

	class Entity {
		private:
			entity_id _id;
		public:
			constexpr explicit Entity(entity_id id) : _id{ id } {}
			constexpr Entity() : _id{ id::invalid_id } {}
			constexpr entity_id get_id() const { return _id; }
			constexpr bool is_valid() const { return id::is_valid(_id); }
			transform::Component transform() const;
		};
}
