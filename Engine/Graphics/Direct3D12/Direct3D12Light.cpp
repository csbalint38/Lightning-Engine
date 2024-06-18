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

			private:
				util::free_list<LightOwner> _owners;
				util::vector<hlsl::DirectionalLightParameters> _non_cullable_lights;
				util::vector<light_id> _non_cullable_owners;
		};

		std::unordered_map<u64, LightSet> light_sets;

		graphics::Light create(LightInitInfo info) {
			return {};
		}

		void remove(light_id id, u64 light_set_key) {

		}

		void set_parameter(light_id id, u64 light_set_key, LightParameter::Parameter parameter, const void* const data, u32 data_size) {

		}

		void get_parameter(light_id id, u64 light_set_key, LightParameter::Parameter parameter, void* const data, u32 data_size) {

		}
	}
}