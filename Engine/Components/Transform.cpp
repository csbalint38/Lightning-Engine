#include "Entity.h"
#include "Transform.h"

namespace lightning::transform {
	namespace {
		util::vector<math::v3> positions;
		util::vector<math::v3> orientations;
		util::vector<math::v4> rotations;
		util::vector<math::v3> scales;
	}

	math::v3 calculate_orientation(math::v4 rotation) {
		using namespace DirectX;
		XMVECTOR rotation_quat{ XMLoadFloat4(&rotation) };
		XMVECTOR front{ XMVectorSet(0.f, 0.f, 1.f, 0.f) };
		math::v3 orientation;
		XMStoreFloat3(&orientation, XMVector3Rotate(front, rotation_quat));
		return orientation;
	}

	Component create(InitInfo info, game_entity::Entity entity) {
		assert(entity.is_valid());
		const id::id_type entity_index{ id::index(entity.get_id()) };

		if (positions.size() > entity_index) {
			math::v4 rotation{ info.rotation };
			rotations[entity_index] = rotation;
			orientations[entity_index] = calculate_orientation(rotation);
			positions[entity_index] = math::v3{ info.position };
			scales[entity_index] = math::v3{ info.scale };
		}
		else {
			assert(positions.size() == entity_index);
			rotations.emplace_back(info.rotation);
			orientations.emplace_back(calculate_orientation(math::v4{ info.rotation }));
			positions.emplace_back(info.position);
			scales.emplace_back(info.scale);
		}
		return Component{ transform_id{ entity.get_id()} };
	}

	void remove([[maybe_unused]]Component component) {
		assert(component.is_valid());
	}

	math::v3 Component::position() const {
		assert(is_valid());
		return positions[id::index(_id)];
	}
	
	math::v4 Component::rotation() const {
		assert(is_valid());
		return rotations[id::index(_id)];
	}

	math::v3 Component::orientation() const {
		assert(is_valid());
		return orientations[id::index(_id)];
	}

	math::v3 Component::scale() const {
		assert(is_valid());
		return scales[id::index(_id)];
	}
}