#include "ShaderCompilation.h"
#include "Graphics/Direct3D12/Direct3D12Core.h"
#include "Graphics/Direct3D12/Direct3D12Shaders.h"

#include<dxcapi.h>
#include<filesystem>
#include<d3d12shader.h>
#include<fstream>

using namespace lightning;
using namespace lightning::graphics::direct3d12::shaders;
using namespace Microsoft::WRL;

namespace {
	struct ShaderFileInfo {
		const char* file;
		const char* function;
		EngineShader::Id id;
		ShaderType::Type type;
	};

	constexpr ShaderFileInfo shader_files[]{
		{ "FullscreenTriangle.hlsl", "FullscreenTriangleVS", EngineShader::FULLSCREEN_TRIANGLE_VS, ShaderType::VERTEX },
	};

	static_assert(_countof(shader_files) == EngineShader::count);

	constexpr const char* shaders_source_path{ "../../Engine/Graphics/Direct3D12/Shaders/" };

	decltype(auto) get_engine_shaders_path() {
		return std::filesystem::absolute(graphics::get_engine_shaders_path(graphics::GraphicsPlatform::DIRECT3D12));
	}

	bool compiled_shaders_are_up_to_date() {
		auto engine_shaders_path = get_engine_shaders_path();
		if (!std::filesystem::exists(engine_shaders_path)) return false;
		auto shaders_compilation_time = std::filesystem::last_write_time(engine_shaders_path);

		std::filesystem::path path{};
		std::filesystem::path full_path{};

		for (u32 i{ 0 }; i < EngineShader::count; ++i) {
			auto& info = shader_files[i];
			path = shaders_source_path;
			path += info.file;
			full_path = std::filesystem::absolute(path);
			if (!std::filesystem::exists(full_path)) return false;

			auto shader_file_time = std::filesystem::last_write_time(full_path);
			if (shader_file_time > shaders_compilation_time) return false;
		}

		return true;
	}

	bool save_compiled_shaders(util::vector<ComPtr<IDxcBlob>>& shaders) {
		auto engine_shaders_path = get_engine_shaders_path();
		std::filesystem::create_directories(engine_shaders_path.parent_path());
		std::ofstream file(engine_shaders_path, std::ios::out | std::ios::binary);
		if (!file || !std::filesystem::exists(engine_shaders_path)) {
			file.close();
			return false;
		}

		for (auto& shader : shaders) {
			const D3D12_SHADER_BYTECODE byte_code{ shader->GetBufferPointer(), shader->GetBufferSize() };
			file.write((char*)&byte_code.BytecodeLength, sizeof(byte_code.BytecodeLength));
			file.write((char*)&byte_code.pShaderBytecode, sizeof(byte_code.BytecodeLength));
		}

		file.close();

		return true;
	}
}

bool compile_shaders() {
	if (compiled_shaders_are_up_to_date()) return true;
	util::vector<ComPtr<IDxcBlob>> shaders;
	std::filesystem::path path{};
	std::filesystem::path full_path{};

	for (u32 i{ 0 }; i < EngineShader::count; ++i) {
		auto& info = shader_files[i];
		path = shaders_source_path;
		path += info.file;
		full_path = std::filesystem::absolute(path);

		if (!std::filesystem::exists(full_path)) return false;

		ComPtr<IDxcBlob> compiled_shader{ };

		if (compiled_shader->GetBufferPointer() && compiled_shader->GetBufferSize()) {
			shaders.emplace_back(std::move(compiled_shader));
		}
		else return false;
	}

	return save_compiled_shaders(shaders);
}