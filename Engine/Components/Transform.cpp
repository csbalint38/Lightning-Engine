#include "Transform.h"

namespace lightning::transform {
	namespace {

		#ifdef _MSC_VER
		#pragma warning(push)
		#pragma warning(disable : 4201) // C4201: nonstandard extension used: nameless struct/union
		#endif
		struct LocalFrame {
			union {
				struct {
					math::v3 right;
					math::v3 up;
					math::v3 front;
				};
				math::m3x3 frame;
			};

			LocalFrame() : right{ 1.f, 0.f, 0.f }, up{ 0.f, 1.f, 0.f }, front{ 0.f, 0.f, 1.f } {}
		};
		#ifdef _MSC_VER
		#pragma warning(pop)
		#endif

		util::vector<math::m4x4> to_world;
		util::vector<math::m4x4> inv_world;
		util::vector<math::v3> positions;
		util::vector<LocalFrame> local_frames;
		util::vector<math::v4> rotations;
		util::vector<math::v3> scales;
		util::vector<u8> has_transform;
		util::vector<u8> changes_from_previous_frame;
		u8 read_write_flags;
	}

	void calculate_transform_matrices(id::id_type index) {
		assert(positions.size() > index);
		assert(rotations.size() > index);
		assert(scales.size() > index);

		using namespace DirectX;
		XMVECTOR r{ XMLoadFloat4(&rotations[index]) };
		XMVECTOR p{ XMLoadFloat3(&positions[index]) };
		XMVECTOR s{ XMLoadFloat3(&scales[index]) };

		XMMATRIX world{ XMMatrixAffineTransformation(s, XMQuaternionIdentity(), r, p) };
		XMStoreFloat4x4(&to_world[index], world);

		world.r[3] = XMVectorSet(0.f, 0.f, 0.f, 1.f);
		XMMATRIX inverse_world{ XMMatrixInverse(nullptr, world) };
		XMStoreFloat4x4(&inv_world[index], inverse_world);

		has_transform[index] = 1;
	}

	void calculate_local_frame(const math::v4& rotation, LocalFrame& result) {
		using namespace DirectX;

		LocalFrame frame{ };

		XMVECTOR right{ XMLoadFloat3(&frame.right) };
		XMVECTOR up{ XMLoadFloat3(&frame.up) };
		XMVECTOR front{ XMLoadFloat3(&frame.front) };
		XMVECTOR rotation_quat{ XMLoadFloat4(&rotation) };

		right = XMVector3Normalize(XMVector3Rotate(right, rotation_quat));
		up = XMVector3Normalize(XMVector3Rotate(up, rotation_quat));
		front = XMVector3Normalize(XMVector3Cross(right, up));

		XMStoreFloat3(&result.right, right);
		XMStoreFloat3(&result.up, up);
		XMStoreFloat3(&result.front, front);
	}

	void set_rotation(transform_id id, const math::v4& rotaion_quaternion) {
		const u32 index{ id::index(id) };
		rotations[index] = rotaion_quaternion;

		calculate_local_frame(rotaion_quaternion, local_frames[index]);

		has_transform[index] = 0;
		changes_from_previous_frame[index] |= ComponentFlags::ROTATION;
	}

	void set_position(transform_id id, const math::v3& position) {
		const u32 index{ id::index(id) };
		positions[index] = position;
		has_transform[index] = 0;
		changes_from_previous_frame[index] |= ComponentFlags::POSITION;
	}

	void set_scale(transform_id id, const math::v3& scale) {
		const u32 index{ id::index(id) };
		scales[index] = scale;
		has_transform[index] = 0;
		changes_from_previous_frame[index] |= ComponentFlags::SCALE;
	}

	Component create(InitInfo info, game_entity::Entity entity) {
		assert(entity.is_valid());
		const id::id_type entity_index{ id::index(entity.get_id()) };

		if (positions.size() > entity_index) {
			math::v4 rotation{ info.rotation };
			rotations[entity_index] = rotation;

			calculate_local_frame(rotation, local_frames[entity_index]);

			positions[entity_index] = math::v3{ info.position };
			scales[entity_index] = math::v3{ info.scale };
			has_transform[entity_index] = 0;
			changes_from_previous_frame[entity_index] = (u8)ComponentFlags::ALL;
		}
		else {
			assert(positions.size() == entity_index);
			rotations.emplace_back(info.rotation);
			local_frames.emplace_back();
			
			calculate_local_frame(rotations.back(), local_frames.back());

			positions.emplace_back(info.position);
			scales.emplace_back(info.scale);
			has_transform.emplace_back((u8)0);
			to_world.emplace_back();
			inv_world.emplace_back();
			changes_from_previous_frame.emplace_back((u8)ComponentFlags::ALL);
		}
		return Component{ transform_id{ entity.get_id()} };
	}

	void remove([[maybe_unused]] Component component) {
		assert(component.is_valid());
	}

	void get_transform_matrices(const game_entity::entity_id id, math::m4x4& world, math::m4x4& inverse_world) {
		assert(game_entity::Entity{ id }.is_valid());

		const id::id_type entity_index{ id::index(id) };
		if (!has_transform[entity_index]) {
			calculate_transform_matrices(entity_index);
		}

		world = to_world[entity_index];
		inverse_world = inv_world[entity_index];
	}

	void get_updated_component_flags(const game_entity::entity_id* const ids, u32 count, u8* const flags) {
		assert(ids && count && flags);
		read_write_flags = 1;

		for (u32 i{ 0 }; i < count; ++i) {
			assert(game_entity::Entity{ ids[i] }.is_valid());
			flags[i] = changes_from_previous_frame[id::index(ids[i])];
		}
	}

	void update(const ComponentCache* const cache, u32 count) {
		assert(cache && count);
		if (read_write_flags) {
			memset(changes_from_previous_frame.data(), 0, changes_from_previous_frame.size());
			read_write_flags = 0;
		}

		for (u32 i{ 0 }; i < count; ++i) {
			const ComponentCache& c{ cache[i] };
			assert(Component{ c.id }.is_valid());

			if (c.flags & ComponentFlags::ROTATION) {
				set_rotation(c.id, c.rotation);
			}

			if (c.flags & ComponentFlags::POSITION) {
				set_position(c.id, c.position);
			}

			if (c.flags & ComponentFlags::SCALE) {
				set_scale(c.id, c.scale);
			}
		}
	}

	math::v3 Component::position() const {
		assert(is_valid());
		return positions[id::index(_id)];
	}
	
	math::v4 Component::rotation() const {
		assert(is_valid());
		return rotations[id::index(_id)];
	}

	math::v3 Component::scale() const {
		assert(is_valid());
		return scales[id::index(_id)];
	}

	math::v3 Component::right() const {
		assert(is_valid());

		return local_frames[id::index(_id)].right;
	}

	math::v3 Component::up() const {
		assert(is_valid());

		return local_frames[id::index(_id)].up;
	}

	math::v3 Component::front() const {
		assert(is_valid());

		return local_frames[id::index(_id)].front;
	}

	math::m3x3 Component::local_frame() const {
		assert(is_valid());

		return local_frames[id::index(_id)].frame;
	}

	DirectX::XMVECTOR Component::calculate_local_position(math::v3 delta) const {
		assert(is_valid());

		const id::id_type index{ id::index(_id) };

		using namespace DirectX;

		XMMATRIX frame{ XMLoadFloat3x3(&local_frames[index].frame) };
		XMVECTOR l_pos{ XMLoadFloat3(&delta) };

		l_pos = XMVector3Transform(l_pos, frame);

		XMVECTOR w_pos{ XMLoadFloat3(&positions[index]) };

		return w_pos + l_pos;
	}

	DirectX::XMVECTOR Component::calculate_absolute_rotation(math::v3 rotation) const {
		assert(is_valid());

		return DirectX::XMQuaternionRotationRollPitchYawFromVector(XMLoadFloat3(&rotation));
	}

	DirectX::XMVECTOR Component::calculate_local_rotation(math::v3 delta) const {
		assert(is_valid());

		const id::id_type index{ id::index(_id) };

		using namespace DirectX;

		XMVECTOR d{ DirectX::XMQuaternionRotationRollPitchYawFromVector(XMLoadFloat3(&delta)) };
		XMVECTOR q{ XMLoadFloat4(&rotations[index]) };

		return XMQuaternionMultiply(d, q);
	}

	DirectX::XMVECTOR Component::calculate_world_rotation(math::v3 delta) const {
		assert(is_valid());

		math::v3 axis{ 1.f,0.f,0.f };
		f32 angle{ delta.x };

		if (abs(delta.x) < math::EPSILON) {
			if (abs(delta.z) < math::EPSILON) {
				axis = { 0.f,1.f,0.f };
				angle = delta.y;
			}
			else if (abs(delta.y) < math::EPSILON) {
				axis = { 0.f,0.f,1.f };
				angle = delta.z;
			}
		}

		const id::id_type index{ id::index(_id) };

		using namespace DirectX;

		XMVECTOR a{ XMLoadFloat3(&axis) };
		XMVECTOR d{ XMQuaternionRotationNormal(a, angle) };
		XMVECTOR q{ XMLoadFloat4(&rotations[index]) };

		return XMQuaternionMultiply(q, d);
	}
}