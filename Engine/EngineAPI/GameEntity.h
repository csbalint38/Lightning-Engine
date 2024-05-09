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
			using string_hash = std::hash<std::string>;

			u8 register_script(size_t, script_creator);
			#ifdef USE_WITH_EDITOR
			extern "C" __declspec(dllexport)
			#endif
			script_creator get_script_creator(size_t tag);

			template<class ScriptClass>
			script_ptr create_script(game_entity::Entity entity) {
				assert(entity.is_valid());
				return std::make_unique<ScriptClass>(entity);
			}

			#ifdef USE_WITH_EDITOR
			u8 add_script_name(const char* name);

			#define REGISTER_SCRIPT(TYPE)																															\																																		\
			namespace {																																				\
				const u8 _reg_##TYPE {																																\
					lightning::script::detail::register_script(lightning::script::detail::string_hash()(#TYPE), &lightning::script::detail::create_script<TYPE>)	\
				};																																					\
				const u8 _name_##TYPE																																	\
				{ lightning::script::detail::add_script_name(#TYPE) }																									\
			}

			#else
			#define REGISTER_SCRIPT(TYPE)																															\																																		\
			namespace {																																				\
				const u8 _reg_##TYPE{																																\
					lightning::script::detail::register_script(lightning::script::detail::string_hash()(#TYPE), &lightning::script::detail::create_script<TYPE>)	\
				};																																					\
			}
			#endif
		}
	}
}
