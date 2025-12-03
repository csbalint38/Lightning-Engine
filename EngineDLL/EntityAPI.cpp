#include "CommonHeaders.h"
#include "Id.h"
#include "Common.h"
#include "Components/Entity.h"
#include "Components/Transform.h"
#include "Components/Script.h"
#include "Components/Geometry.h"
#include "Utilities/Threading.h"

using namespace lightning;

math::v4 to_quat(math::v3 angles, bool is_degrees) {
	using namespace DirectX;

	if (is_degrees) angles = math::to_radians(angles);

	math::v4 quat_result{};
	XMVECTOR quat{ XMQuaternionRotationRollPitchYawFromVector(XMLoadFloat3(&angles)) };

	XMStoreFloat4(&quat_result, quat);

	return quat_result;
}

namespace {

	struct TransformComponentDescriptor {
		f32 position[3];
		f32 rotation[3];
		f32 scale[3];

		transform::InitInfo to_init_info() const {
			using namespace DirectX;
			transform::InitInfo info{};
			memcpy(&info.position[0], &position[0], sizeof(position));
			memcpy(&info.scale[0], &position[0], sizeof(position));
			math::v3 rot{ &rotation[0] };
			math::v4 rot_quat{ to_quat(rot, true) };
			memcpy(&info.rotation[0], &rot_quat.x, sizeof(info.rotation));

			return info;
		}
	};

	struct ScriptComponentDescriptor {
		script::detail::script_creator script_creator;

		script::InitInfo to_init_info() const {
			script::InitInfo info{};
			info.script_creator = script_creator;

			return info;
		}
	};

	struct GeometryComponentDescriptor {
		id::id_type geometry_content_id;
		u32 material_count;
		id::id_type* material_ids;

		geometry::InitInfo to_init_info() const {
			geometry::InitInfo info{};

			info.geometry_content_id = geometry_content_id;
			info.material_count = material_count;
			info.material_ids = material_ids;

			return info;
		}
	};

	struct EntityDescriptor {
		TransformComponentDescriptor transform;
		ScriptComponentDescriptor script;
		GeometryComponentDescriptor geometry;
	};

	game_entity::Entity entity_from_id(id::id_type id) {
		return game_entity::Entity{ game_entity::entity_id{id} };
	}
}

util::TicketMutex mutex{};

EDITOR_INTERFACE id::id_type create_game_entity(EntityDescriptor* entity) {
	std::lock_guard lock{ mutex };

	assert(entity);
	EntityDescriptor& desc{ *entity };
	transform::InitInfo transform_info{ desc.transform.to_init_info() };
	script::InitInfo script_info{ desc.script.to_init_info() };
	geometry::InitInfo geometry_info{ desc.geometry.to_init_info() };
	game_entity::EntityInfo entity_info{
		&transform_info,
		&script_info,
		id::is_valid(desc.geometry.geometry_content_id) ? &geometry_info : nullptr,
	};

	return game_entity::create(entity_info).get_id();
}

EDITOR_INTERFACE void remove_game_entity(id::id_type id) {
	std::lock_guard lock{ mutex };

	assert(id::is_valid(id));
	game_entity::remove(game_entity::entity_id{ id });
}

EDITOR_INTERFACE b32 update_component(id::id_type entity_id, EntityDescriptor* e, ComponentType::Type type) {
	std::lock_guard lock{ mutex };

	assert(id::is_valid(entity_id) && e && type != ComponentType::TRANSFORM);

	EntityDescriptor& desc{ *e };
	script::InitInfo script_info{ desc.script.to_init_info() };
	geometry::InitInfo geometry_info{ desc.geometry.to_init_info() };

	game_entity::EntityInfo entity_info{
		nullptr,
		&script_info,
		id::is_valid(desc.geometry.geometry_content_id) ? &geometry_info : nullptr
	};

	return game_entity::update_component(game_entity::entity_id{ entity_id }, entity_info, type) ? 1 : 0;
}

EDITOR_INTERFACE id::id_type get_component_id(id::id_type entity_id, ComponentType::Type type) {
	std::lock_guard lock{ mutex };

	assert(id::is_valid(entity_id));

	game_entity::Entity entity{ game_entity::entity_id{entity_id} };

	switch (type) {
		case ComponentType::TRANSFORM: return entity.transform().get_id();
		case ComponentType::SCRIPT: return entity.script().get_id();
		case ComponentType::GEOMETRY: return entity.geometry().get_id();
		default: return id::invalid_id;
	}
}