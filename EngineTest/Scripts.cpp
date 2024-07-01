#include "Components/Entity.h"
#include "Components/Transform.h"
#include "Components/Script.h"

using namespace lightning;

class RotatorScript;
REGISTER_SCRIPT(RotatorScript);
class RotatorScript : public script::EntityScript {
	public:
		constexpr explicit RotatorScript(game_entity::Entity entity) : script::EntityScript{ entity } {}
		void begin_play() override {}
		void update(float dt) override {
			_angle += 0.25f * dt * math::TWO_PI;
			if (_angle > math::TWO_PI) _angle -= math::TWO_PI;
			math::v3a rot{ 0.f, _angle, 0.f };
			DirectX::XMVECTOR quat{ DirectX::XMQuaternionRotationRollPitchYawFromVector(DirectX::XMLoadFloat3A(&rot)) };
			math::v4 rot_quat{};
			DirectX::XMStoreFloat4(&rot_quat, quat);
			set_rotation(rot_quat);
		}

	private:
		f32 _angle{ 0.f };
};

class TurbineScript;
REGISTER_SCRIPT(TurbineScript);
class TurbineScript : public script::EntityScript {
	public:
		constexpr explicit TurbineScript(game_entity::Entity entity) : script::EntityScript{ entity } {}
		void begin_play() override {}
		void update(float dt) override {
			_angle += .05f * dt * math::TWO_PI;
			if (_angle > math::TWO_PI) _angle += math::TWO_PI;
			math::v3a rot{ 0.f, _angle, 0.f};
			DirectX::XMVECTOR quat{ DirectX::XMQuaternionRotationRollPitchYawFromVector(DirectX::XMLoadFloat3A(&rot)) };
			math::v4 rot_quat{};
			DirectX::XMStoreFloat4(&rot_quat, quat);
			set_rotation(rot_quat);
		}

	private:
		f32 _angle{ 0.f };
};