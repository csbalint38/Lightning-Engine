#include "Direct3D12Light.h"
#include "Shaders/ShaderTypes.h"
#include "EngineAPI/GameEntity.h"

namespace lightning::graphics::direct3d12::light {
	namespace {

		struct LightOwner {
			game_entity::entity_id entity_id{ id::invalid_id };
			u32 data_index;
			graphics::Light::Type type;
			bool is_enabled;
		};

		#if USE_STL_VECTOR
		#define CONSTEXPR
		#else
		#define CONSTEXPR constexpr
		#endif

		class LightSet {
			public:
				constexpr graphics::Light add(const LightInitInfo& info) {
					if (info.type == graphics::Light::DIRECTIONAL) {
						u32 index{ u32_invalid_id };

						for (u32 i{ 0 }; i < _non_cullable_owners.size(); ++i) {
							if (!id::is_valid(_non_cullable_owners[i])) {
								index = i;
								break;
							}
						}

						if (index == u32_invalid_id) {
							index = (u32)_non_cullable_owners.size();
							_non_cullable_owners.emplace_back();
							_non_cullable_lights.emplace_back();
						}

						hlsl::DirectionalLightParameters& params{ _non_cullable_lights[index] };
						params.color = info.color;
						params.intensity = info.intensity;

						LightOwner owner{ game_entity::entity_id{info.entity_id}, index, info.type, info.is_enabled };
						const light_id id{ _owners.add(owner) };
						_non_cullable_owners[index] = id;

						return graphics::Light{ id, info.light_set_key };
					}
				}

				constexpr void remove(light_id id) {
					enable(id, false);

					const LightOwner& owner{ _owners[id] };

					if (owner.type == graphics::Light::DIRECTIONAL) {
						_non_cullable_owners[owner.data_index] = light_id{ id::invalid_id };
					}

					_owners.remove(id);
				}

				constexpr void enable(light_id id, bool is_enabled) {
					_owners[id].is_enabled = is_enabled;

					if (_owners[id].type == graphics::Light::DIRECTIONAL) {
						return;
					}
				}

				constexpr void intensity(light_id id, f32 intensity) {
					if (intensity < 0.f) intensity = 0.f;

					const LightOwner& owner{ _owners[id] };
					const u32 index{ owner.data_index };

					if (owner.type == graphics::Light::DIRECTIONAL) {
						assert(index < _non_cullable_lights.size());
						_non_cullable_lights[index].intensity = intensity;
					}
				}

				constexpr void color(light_id id, math::v3 color) {
					assert(color.x <= 1.f && color.y <= 1.f && color.z <= 1.f);
					assert(color.x >= 0.f && color.y >= 0.f && color.z >= 0.f);

					const LightOwner& owner{ _owners[id] };
					const u32 index{ owner.data_index };

					if (owner.type == graphics::Light::DIRECTIONAL) {
						assert(index < _non_cullable_lights.size());
						_non_cullable_lights[index].color = color;
					}
				}

				constexpr bool is_enabled(light_id id) const { return _owners[id].is_enabled; }

				constexpr f32 intensity(light_id id) const {
					const LightOwner& owner{ _owners[id] };
					const u32 index{ owner.data_index };

					if (owner.type == graphics::Light::DIRECTIONAL) {
						assert(index < _non_cullable_lights.size());
						return _non_cullable_lights[index].intensity;
					}
				}

				constexpr math::v3 color(light_id id) {

					const LightOwner& owner{ _owners[id] };
					const u32 index{ owner.data_index };

					if (owner.type == graphics::Light::DIRECTIONAL) {
						assert(index < _non_cullable_lights.size());
						return _non_cullable_lights[index].color;
					}
				}

				constexpr graphics::Light::Type type(light_id id) const { return _owners[id].type; }
				constexpr id::id_type entity_id(light_id id) const { return _owners[id].entity_id; }

				CONSTEXPR u32 non_cullable_light_count() const {
					u32 count{ 0 };
					for (const auto& id : _non_cullable_owners) {
						if (id::is_valid(id) && _owners[id].is_enabled) ++count;
					}
					return count;
				}

				CONSTEXPR void non_cullable_lights(hlsl::DirectionalLightParameters* const lights, [[maybe_unused]] u32 buffer_size) {
					assert(buffer_size == math::align_size_up<D3D12_CONSTANT_BUFFER_DATA_PLACEMENT_ALIGNMENT>(non_cullable_light_count() * sizeof   (hlsl::DirectionalLightParameters)));

					const u32 count{ (u32)_non_cullable_owners.size() };
					u32 index{ 0 };
					for (u32 i{ 0 }; i < count; ++i) {
						if (!id::is_valid(_non_cullable_owners[i])) continue;

						const LightOwner& owner{ _owners[_non_cullable_owners[i]] };
						if (owner.is_enabled) {
							assert(_owners[_non_cullable_owners[i]].data_index == i);
							lights[index] = _non_cullable_lights[i];
							++index;
						}
					}
				}

			private:
				util::free_list<LightOwner> _owners;
				util::vector<hlsl::DirectionalLightParameters> _non_cullable_lights;
				util::vector<light_id> _non_cullable_owners;
		};
		#undef CONSTEXPR
	}

	std::unordered_map<u64, LightSet> light_sets;

	constexpr void set_is_enabled(LightSet& set, light_id id, const void* const data, [[maybe_unused]] u32 size) {
		bool is_enabled{ *(bool*)data };
		assert(sizeof(is_enabled) == size);
		set.enable(id, is_enabled);
	}

	constexpr void set_intensity(LightSet& set, light_id id, const void* const data, [[maybe_unused]] u32 size) {
		f32 intensity{ *(f32*)data };
		assert(sizeof(intensity) == size);
		set.intensity(id, intensity);
	}

	constexpr void set_color(LightSet& set, light_id id, const void* const data, [[maybe_unused]] u32 size) {
		math::v3 color{ *(math::v3*)data };
		assert(sizeof(color) == size);
		set.color(id, color);
	}

	constexpr void get_is_enabled(LightSet& set, light_id id, void* const data, [[maybe_unused]] u32 size) {
		bool* const is_enabled{ (bool* const)data };
		assert(sizeof(bool) == size);
		*is_enabled = set.is_enabled(id);
	}

	constexpr void get_intensity(LightSet& set, light_id id, void* const data, [[maybe_unused]] u32 size) {
		f32* const intensity{ (f32* const)data };
		assert(sizeof(f32) == size);
		*intensity = set.intensity(id);
	}

	constexpr void get_color(LightSet& set, light_id id, void* const data, [[maybe_unused]] u32 size) {
		math::v3* const color{ (math::v3* const)data };
		assert(sizeof(math::v3) == size);
		*color = set.color(id);
	}

	constexpr void get_type(LightSet& set, light_id id, void* const data, [[maybe_unused]] u32 size) {
		graphics::Light::Type* const type{ (graphics::Light::Type* const)data };
		assert(sizeof(graphics::Light::Type) == size);
		*type = set.type(id);
	}

	constexpr void get_entity_id(LightSet& set, light_id id, void* const data, [[maybe_unused]] u32 size) {
		id::id_type* const entity_id{ (id::id_type* const)data };
		assert(sizeof(id::id_type) == size);
		*entity_id = set.entity_id(id);
	}

	constexpr void empty_set(LightSet&, light_id, const void* const, u32) {}

	using set_function = void(*)(LightSet&, light_id, const void* const, u32);
	using get_function = void(*)(LightSet&, light_id, void* const, u32);

	constexpr set_function set_functions[]{
		set_is_enabled,
		set_intensity,
		set_color,
		empty_set,
		empty_set
	};

	static_assert(_countof(set_functions) == graphics::LightParameter::count);

	constexpr get_function get_functions[]{
		get_is_enabled,
		get_intensity,
		get_color,
		get_type,
		get_entity_id
	};

	static_assert(_countof(get_functions) == graphics::LightParameter::count);

	graphics::Light create(LightInitInfo info) {
		assert(id::is_valid(info.entity_id));
		return light_sets[info.light_set_key].add(info);
	}

	void remove(light_id id, u64 light_set_key) { light_sets[light_set_key].remove(id); }

	void set_parameter(light_id id, u64 light_set_key, LightParameter::Parameter parameter, const void* const data, u32 data_size) {
		assert(data && data_size);
		assert(parameter < LightParameter::count && set_functions[parameter] != empty_set);
		set_functions[parameter](light_sets[light_set_key], id, data, data_size);
	}

	void get_parameter(light_id id, u64 light_set_key, LightParameter::Parameter parameter, void* const data, u32 data_size) {
		assert(data && data_size);
		assert(parameter < LightParameter::count);
		get_functions[parameter](light_sets[light_set_key], id, data, data_size);
	}
}