#include "Components/Entity.h"
#include "Components/Transform.h"
#include "Components/Script.h"
#include "EngineAPI/Input.h"

using namespace lightning;

class RotatorScript;
REGISTER_SCRIPT(RotatorScript);
class RotatorScript : public script::EntityScript {
	public:
		constexpr explicit RotatorScript(game_entity::Entity entity) : script::EntityScript{ entity } {}
		void begin_play() override {}
		void update(f32 dt) override {
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
		void update(f32 dt) override {
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

class CameraScript;
REGISTER_SCRIPT(CameraScript);
class CameraScript : public script::EntityScript {
public:
	explicit CameraScript(game_entity::Entity entity) : script::EntityScript{ entity } {
		_input_system.add_handler(input::InputSource::MOUSE, this, &CameraScript::mouse_move);

		math::v3 pos{ position() };
		_desired_position = _position = DirectX::XMLoadFloat3(&pos);

		math::v3 dir{ orientation() };
		f32 theta{ DirectX::XMScalarACos(dir.y) };
		f32 phi{ std::atan2(-dir.z, dir.x) };
		math::v3 rot{ theta - math::HALF_PI, phi + HALF_PI, 0.f };
		_desired_spherical = _spherical = DirectX::XMLoadFloat3(&rot);
	}

	void begin_play() override {}
	void update(f32 dt) override {
		_dt = dt;

		math::v3 move{};
		input::InputValue value;
		constexpr input::InputSource::Type kb{ input::InputSource::KEYBOARD };
		input::get(kb, input::InputCode::KEY_W, value);
		move.z += value.current.x;
		input::get(kb, input::InputCode::KEY_A, value);
		move.z -= value.current.x;
		input::get(kb, input::InputCode::KEY_S, value);
		move.x += value.current.x;
		input::get(kb, input::InputCode::KEY_D, value);
		move.x -= value.current.x;
		input::get(kb, input::InputCode::KEY_Q, value);
		move.y -= value.current.x;
		input::get(kb, input::InputCode::KEY_E, value);
		move.y += value.current.x;

		if (!(math::is_equal(move.x, 0.f) && math::is_equal(move.y, 0.f) && math::is_equal(move.z, 0.f))) {
			using namespace DirectX;

			math::v4 rot{ rotation() };
			XMVECTOR d{ XMVector3Rotate(XMLoadFloat3(&move) * 0.2f, XMLoadFloat4(&rot)) };
			_desired_position += d;
			_move_position = true;
		}

		if (_move_position || _move_rotation) {
			seek_camera();
		}
	}

private:

	void mouse_move(input::InputSource::Type type, input::InputCode::Code code, const input::InputValue& mouse_pos) {
		if (code == input::InputCode::MousePosition) {
			input::InputValue value;
			input::get(input::InputSource::MOUSE, input::InputCode::MOUSE_LEFT, value);

			if (value.current.z == 0.f) return;

			const f32 scale{ 0.005f };
			const f32 dx{ (mouse_pos.current.x - mouse_pos.previous.x) * scale };
			const f32 dy{ (mouse_pos.current.y - mouse_pos.previous.y) * scale };

			math::v3 spherical;
			DirectX::XMStoreFloat3(&spherical, _desired_spherical);
			spherical.x += dy;
			spherical.y -= dx;
			spherical.x = math.clamp(spherical.x, 0.0001f - math::HALF_PI, math::HALF_PI - 0.0001f);

			_desired_spherical = DirectX::XMLoadFloat3(&spherical);
			_move_rotation = true;

		}
	}

	void seek_camera() {
		using namespace DirectX;
		
		XMVECTOR p{ _desired_position - _position };
		XMVECTOR o{ _desired_spherical - _spherical };

		_move_position = (XMVectorGetX(XMVector3Length(p)) > 1e-4f);
		_move_rotation = (XMVectorGetX(XMVector3Length(o)) > 1e-4f);

		const f32 scale{ .2f * _dt / 0.016667f };

		if (_move_position) {
			_position += (p * scale);
			math::v3 new_pos;
			XMStoreFloat3(&new_pos, _position);
			set_position(new_pos);
		}

		if (_move_rotation) {
			_spherical += (o * scale);
			math::v3 new_rot;
			XMStoreFloat3(&new_rot, _spherical);
			new_rot.x = math.clamp(new_rot.x, 0.0001f - math::HALF_PI, math::HALF_PI - 0.0001f);
			_spherical = DirctX::XMLoadFloat3(&new_rot);

			DirectX::XMVECTOR quat{ DirectX::XMQuaternionRotationRollPitchYawFromVector(_spherical) };
			math::v4 rot_quat;
			DirectX::XMStoreFloat4(&rot_quat, quat);
			set_rotation(rot_quat);
		}
	}

	input::InputSystem<CameraScript> _input_system{};

	DirectX::XMVECTOR _desired_position;
	DirectX::XMVECTOR _position;
	DirectX::XMVECTOR _desired_spherical;
	DirectX::XMVECTOR _spherical;
	f32 _dt;
	bool _move_position{ false };
	bool _move_rotation{ false };
};