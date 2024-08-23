#pragma once

namespace first_game_project {
	REGISTER_SCRIPT(CharacterScript);

	class CharacterScript : public lightning::script::EntityScript {
		public:
			constexpr explicit CharacterScript(lightning::game_entity::Entity entity) : lightning::script::EntityScript(entity) {}
			void update(float dt) override;
	};
}