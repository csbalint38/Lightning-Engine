#pragma once

#include "..\Components\ComponentsCommonHeaders.h"
#include "TransformComponent.h"
#include "ScriptComponent.h"

namespace lightning {
	namespace game_entity {

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
				script::Component script() const;
		};
	}

	namespace script {
		class EntityScript : public game_entity::Entity {
			public:
				virtual ~EntityScript() = default;
				virtual void begin_play() {}
				virtual void update(float) {}
			protected:
				constexpr explicit EntityScript(game_entity::Entity entity) : game_entity::Entity{ entity.get_id() } {}
		};

		namespace detail {
			using script_ptr = std::unique_ptr<EntityScript>;
			using script_creator = script_ptr(*)(game_entity::Entity entity);

			template<class ScriptClass>
			script_ptr create_script(game_entity::Entity entity) {
				assert(entity.is_valid());
				return std::make_unique<ScriptClass>(entity);
			}
		}
	}
}
