#include "EngineAPI/GameEntity.h"
#include "EngineAPI/Light.h"
#include "EngineAPI/TransformComponent.h"
#include "Graphics/Renderer.h"

#define RANDOM_LIGHTS 1

using namespace lightning;

game_entity::Entity create_one_game_entity(math::v3 position, math::v3 rotation, const char* script_name);
void remove_game_entity(game_entity::entity_id id);

namespace {
	const u64 left_set{ 0 };
	const u64 right_set{ 1 };
	constexpr f32 inv_rand_max{ 1.f / RAND_MAX };

	util::vector<graphics::Light> lights;

	constexpr math::v3 rgb_to_color(u8 r, u8 g, u8 b) { return { r / 255.f, g / 255.f, b / 255.f }; }
	f32 random(f32 min = 0.f) { return std::max(min, rand() * inv_rand_max); }

	void create_light(math::v3 position, math::v3 rotation, graphics::Light::Type type, u64 light_set_key) {
		game_entity::entity_id entity_id{ create_one_game_entity(position, rotation, nullptr).get_id() };

		graphics::LightInitInfo info{};
		info.entity_id = entity_id;
		info.type = type;
		info.light_set_key = light_set_key;
		info.intensity = 1.f;
		info.color = { random(.2f), random(.2f), random(.2f) };
		
		#if RANDOM_LIGHTS
		if (type == graphics::Light::POINT) {
			info.point_params.range = random(.5f) * 2.f;
			info.point_params.attenuation = { 1, 1, 1 };
		}
		else if (type == graphics::Light::SPOT) {
			info.spot_params.range = random(.5f) * 2.f;
			info.spot_params.umbra = (random(.5f) - .4f) * math::PI;
			info.spot_params.penumbra = info.spot_params.umbra + (.1f * math::PI);
			info.spot_params.attenuation = { 1, 1, 1 };
		}
		#else
		if (type == garaphics::Light::POINT) {
			info.point_params.range = 1.f;
			info.point_params.attenuation = { 1, 1, 1 };
		}
		else if (type == graphics::Light::SPOT) {
			info.spot_params.range = 2.f;
			info.spot_params.umbra = .1f * math::PI;
			info.spot_params.penumbra = info.spot_params.umbra + (.1f * math::PI);
			info.spot_params.attenuation = { 1, 1, 1 };
		}
		#endif

		graphics::Light light{ graphics::create_light(info) };
		assert(light.is_valid());
		lights.push_back(light);
	}
}

void generate_lights() {
	graphics::LightInitInfo info{};
	info.entity_id = create_one_game_entity({}, { 0, 0, 0 }, nullptr).get_id();
	info.type = graphics::Light::DIRECTIONAL;
	info.light_set_key = left_set;
	info.intensity = 1.f;
	info.color = rgb_to_color(174, 174, 174);

	lights.emplace_back(graphics::create_light(info));

	info.entity_id = create_one_game_entity({}, { math::PI * .5f, 0, 0 }, nullptr).get_id();
	info.color = rgb_to_color(17, 27, 48);
	lights.emplace_back(graphics::create_light(info));

	info.entity_id = create_one_game_entity({}, { -math::PI * .5f, 0, 0 }, nullptr).get_id();
	info.color = rgb_to_color(63, 47, 30);
	lights.emplace_back(graphics::create_light(info));

	info.entity_id = create_one_game_entity({}, { 0, 0, 0 }, nullptr).get_id();
	info.light_set_key = right_set;
	info.color = rgb_to_color(150, 100, 200);
	lights.emplace_back(graphics::create_light(info));

	info.entity_id = create_one_game_entity({}, { math::PI * .5f, 0, 0 }, nullptr).get_id();
	info.color = rgb_to_color(17, 27, 48);
	lights.emplace_back(graphics::create_light(info));

	info.entity_id = create_one_game_entity({}, { -math::PI * .5f, 0, 0 }, nullptr).get_id();
	info.color = rgb_to_color(63, 47, 130);
	lights.emplace_back(graphics::create_light(info));

	#if !RANDOM_LIGHTS
	create_light({ 0, -3, 0 }, {}, graphics::Light::POINT, left_set);
	create_light({ 0, 0, 1 }, {}, graphics::Light::POINT, left_set);
	create_light({ 0, 3, 2.5f }, {}, graphics::Light::POINT, left_set);
	create_light({ 0, 0, 7 }, { 0, 3.14f, 0 }, graphics::Light::SPOT, left_set);
	#else
	srand(17);

	constexpr math::v3 scale{ 1.f, .5f, 1.f };
	constexpr s32 dim{ 5 };
	for (s32 x{ -dim }; x < dim; ++x) {
		for (s32 y{ 0 }; y < 2 * dim; ++y) {
			for (s32 z{ -dim }; z < dim; ++z) {
				create_light({ (f32)(x * scale.x), (f32)(y * scale.y), (f32)(z * scale.z) }, {3.14f, random(), 0.f}, random() > .5f ? graphics::Light::SPOT : graphics::Light::POINT, left_set);
				create_light({ (f32)(x * scale.x), (f32)(y * scale.y), (f32)(z * scale.z) }, {3.14f, random(), 0.f}, random() > .5f ? graphics::Light::SPOT : graphics::Light::POINT, right_set);
			}
		}
	}
	#endif
}

void remove_lights() {
	for (auto& light : lights) {
		const game_entity::entity_id id{ light.entity_id() };
		graphics::remove_light(light.get_id(), light.light_set_key());
		remove_game_entity(id);
	}

	lights.clear();
}