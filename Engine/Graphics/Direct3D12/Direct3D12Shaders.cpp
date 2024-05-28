#include "Direct3D12Shaders.h"
#include "Content/ContentLoader.h"

namespace lightning::graphics::direct3d12::shaders {
	namespace {
		typedef struct CompiledShaders {
			u64 size;
			const u8* byte_code;
		} const* compiled_shader_ptr;

		compiled_shader_ptr engine_shaders[EngineShader::count]{};
		std::unique_ptr<u8[]> shaders_blob{};

		bool load_engine_shaders() {
			assert(!shaders_blob);
			u64 size{ 0 };
			bool result{ content::load_engine_shaders(shaders_blob, size) };
			assert(shaders_blob && size);

			u64 offset{ 0 };
			u32 index{ 0 };

			while (offset < size && result) {
				assert(index < EngineShader::count);
				compiled_shader_ptr& shader{ engine_shaders[index] };
				assert(!shader);

				result &= index < EngineShader::count && !shader;

				if (!result) break;

				shader = reinterpret_cast<const compiled_shader_ptr>(&shaders_blob[offset]);
				offset += sizeof(u64) + shader->size;
				++index;
			}
			assert(offset == size && index == EngineShader::count);

			return result;
		}
	}

	bool initialize() {
		return load_engine_shaders();
	}

	void shutdown() {
		for (u32 i{ 0 }; i < EngineShader::count; ++i) {
			engine_shaders[i] = {};
		}
		shaders_blob.reset();
	}

	D3D12_SHADER_BYTECODE get_engine_shader(EngineShader::Id id) {
		assert(id < EngineShader::count);
		const compiled_shader_ptr shader{ engine_shaders[id] };
		assert(shader && shader->size);

		return { &shader->byte_code, shader->size };
	}
}