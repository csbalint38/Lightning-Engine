#include "Direct3D12Light.h"
#include "Direct3D12Core.h"
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

			void update_transforms() {
				for (const auto& id : _non_cullable_owners) {
					if (!id::is_valid(id)) continue;

					const LightOwner& owner{ _owners[id] };
					if (owner.is_enabled) {
						const game_entity::Entity entity{ game_entity::entity_id{ owner.entity_id } };
						hlsl::DirectionalLightParameters& params{ _non_cullable_lights[owner.data_index] };
						params.direction = entity.orientation();
					}
				}
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
				assert(buffer_size == math::align_size_up<D3D12_CONSTANT_BUFFER_DATA_PLACEMENT_ALIGNMENT>(non_cullable_light_count() * sizeof(hlsl::DirectionalLightParameters)));

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

			constexpr bool has_lights() const { return _owners.size() > 0; }

		private:
			util::free_list<LightOwner> _owners;
			util::vector<hlsl::DirectionalLightParameters> _non_cullable_lights;
			util::vector<light_id> _non_cullable_owners;
		};

		class D3D12LightBuffer {
		public:
			D3D12LightBuffer() = default;
			CONSTEXPR void update_light_buffers(LightSet& set, u64 light_set_key, u32 frame_index) {
				u32 sizes[LightBuffer::count]{};
				sizes[LightBuffer::NON_CULLABLE_LIGHT] = set.non_cullable_light_count() * sizeof(hlsl::DirectionalLightParameters);

				u32 current_sizes[LightBuffer::count]{};
				current_sizes[LightBuffer::NON_CULLABLE_LIGHT] = _buffers[LightBuffer::NON_CULLABLE_LIGHT].buffer.size();

				if (current_sizes[LightBuffer::NON_CULLABLE_LIGHT] < sizes[LightBuffer::NON_CULLABLE_LIGHT]) {
					resize_buffer(LightBuffer::NON_CULLABLE_LIGHT, sizes[LightBuffer::NON_CULLABLE_LIGHT], frame_index);
				}

				set.non_cullable_lights((hlsl::DirectionalLightParameters* const)_buffers[LightBuffer::NON_CULLABLE_LIGHT].cpu_address, _buffers[LightBuffer::NON_CULLABLE_LIGHT].buffer.size());
			}

			constexpr void release() {
				for (u32 i{ 0 }; i < LightBuffer::count; ++i) {
					_buffers[i].buffer.release();
					_buffers[i].cpu_address = nullptr;
				}
			}

			constexpr D3D12_GPU_VIRTUAL_ADDRESS non_cullable_lights() const { return _buffers[LightBuffer::NON_CULLABLE_LIGHT].buffer.gpu_address(); }

		private:
			struct LightBuffer {
				enum Type : u32 {
					NON_CULLABLE_LIGHT,
					CULLABLE_LIGHT,
					CULLING_INFO,

					count
				};

				D3D12Buffer buffer{};
				u8* cpu_address{ nullptr };
			};

			void resize_buffer(LightBuffer::Type type, u32 size, [[maybe_unused]] u32 frame_index) {
				assert(type < LightBuffer::count);
				if (!size) return;

				_buffers[type].buffer.release();
				_buffers[type].buffer = D3D12Buffer{ ConstantBuffer::get_default_init_info(size),true };
				NAME_D3D12_OBJECT_INDEXED(_buffers[type].buffer.buffer(), frame_index, type == LightBuffer::NON_CULLABLE_LIGHT ? L"Non-cullable Light Buffer" : type == LightBuffer::CULLABLE_LIGHT ? L"Cullable Light Buffer" : L"Light Culling Info Buffer");

				D3D12_RANGE range{};
				DXCall(_buffers[type].buffer.buffer()->Map(0, &range, (void**)(&_buffers[type].cpu_address)));
				assert(_buffers[type].cpu_address);
			}

			LightBuffer _buffers[LightBuffer::count];
			u64 _current_light_set_key{ 0 };
		};

		#undef CONSTEXPR

		std::unordered_map<u64, LightSet> light_sets;
		D3D12LightBuffer light_buffers[FRAME_BUFFER_COUNT];

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
	}

	bool initialize() { return true; }

	void shutdown() {
		assert([] {
			bool has_lights{ false };
			for (const auto& it : light_sets) {
				has_lights |= it.second.has_lights();
			}
			return !has_lights;
		}());

		for (u32 i{ 0 }; i < FRAME_BUFFER_COUNT; ++i) {
			light_buffers[i].release();
		}
	}

	graphics::Light create(LightInitInfo info) {
		assert(id::is_valid(info.entity_id));
		return light_sets[info.light_set_key].add(info);
	}

	void remove(light_id id, u64 light_set_key) {
		assert(light_sets.count(light_set_key));
		light_sets[light_set_key].remove(id);
	}

	void set_parameter(light_id id, u64 light_set_key, LightParameter::Parameter parameter, const void* const data, u32 data_size) {
		assert(data && data_size);
		assert(light_sets.count(light_set_key));
		assert(parameter < LightParameter::count && set_functions[parameter] != empty_set);
		set_functions[parameter](light_sets[light_set_key], id, data, data_size);
	}

	void get_parameter(light_id id, u64 light_set_key, LightParameter::Parameter parameter, void* const data, u32 data_size) {
		assert(data && data_size);
		assert(light_sets.count(light_set_key));
		assert(parameter < LightParameter::count);
		get_functions[parameter](light_sets[light_set_key], id, data, data_size);
	}

	void update_light_buffers(const D3D12FrameInfo& info) {
		const u64 light_set_key{ info.info->light_set_key };
		assert(light_sets.count(light_set_key));

		LightSet& set{ light_sets[light_set_key] };

		if (!set.has_lights()) return;

		set.update_transforms();
		const u32 frame_index{ info.frame_index };
		D3D12LightBuffer& light_buffer{ light_buffers[frame_index] };
		light_buffer.update_light_buffers(set, light_set_key, frame_index);
	}

	D3D12_GPU_VIRTUAL_ADDRESS non_cullable_light_buffer(u32 frame_index) {
		const D3D12LightBuffer& light_buffer{ light_buffers[frame_index] };
		return light_buffer.non_cullable_lights();
	}

	u32 non_cullable_light_count(u64 light_set_key) {
		assert(light_sets.count(light_set_key));
		return light_sets[light_set_key].non_cullable_light_count();
	}
}