#include "ShaderCompilation.h"
#include "Graphics/Direct3D12/Direct3D12Core.h"
#include "Graphics/Direct3D12/Direct3D12Shaders.h"

#include <dxcapi.h>
#include <filesystem>
#include <d3d12shader.h>
#include <fstream>

#pragma comment(lib, "../packages/DirectXShaderCompiler/lib/x64/dxcompiler.lib")

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
		{ "FullscreenTriangle.hlsl", "fullscreen_triangle_vs", EngineShader::FULLSCREEN_TRIANGLE_VS, ShaderType::VERTEX },
		{ "FillColor.hlsl", "fill_color_ps", EngineShader::FILL_COLOR_PS, ShaderType::PIXEL },
		{ "PostProcess.hlsl", "post_process_ps", EngineShader::POST_PROCESS_PS, ShaderType::PIXEL },
	};

	std::wstring to_wstring(const char* c) {
		std::string s{ c };
		return { s.begin(), s.end() };
	}

	static_assert(_countof(shader_files) == EngineShader::count);

	constexpr const char* shaders_source_path{ "../../Engine/Graphics/Direct3D12/Shaders/" };

	class ShaderCompiler {
	public:
		ShaderCompiler() {

			HRESULT hr{ S_OK };
			DXCall(hr = DxcCreateInstance2(nullptr, CLSID_DxcCompiler, IID_PPV_ARGS(&_compiler)));
			if (FAILED(hr)) return;
			DXCall(hr = DxcCreateInstance2(nullptr, CLSID_DxcUtils, IID_PPV_ARGS(&_utils)));
			if (FAILED(hr)) return;
			DXCall(hr = _utils->CreateDefaultIncludeHandler(&_include_handler));
			if (FAILED(hr)) return;
		}

		DISABLE_COPY_AND_MOVE(ShaderCompiler);

		IDxcBlob* compile(ShaderFileInfo info, std::filesystem::path full_path) {
			assert(_compiler && _utils && _include_handler);
			HRESULT hr{ S_OK };

			ComPtr<IDxcBlobEncoding> source_blob{ nullptr };
			DXCall(hr = _utils->LoadFile(full_path.c_str(), nullptr, &source_blob));
			if (FAILED(hr)) return nullptr;
			assert(source_blob && source_blob->GetBufferSize());

			std::wstring file{ to_wstring(info.file) };
			std::wstring func{ to_wstring(info.function) };
			std::wstring prof{ to_wstring(_profile_strings[(u32)info.type]) };
			std::wstring inc{ to_wstring(shaders_source_path) };

			LPCWSTR args[]{
				file.c_str(),		// Shader source file for error reporting
				L"-E",
				func.c_str(),		// Entry function
				L"-T",
				prof.c_str(),		// Target profile
				L"-I",
				inc.c_str(),		// Include path
				DXC_ARG_ALL_RESOURCES_BOUND,
				#if _DEBUG
				DXC_ARG_DEBUG,
				DXC_ARG_SKIP_OPTIMIZATIONS,
				#else
				DXC_ARG_OPTIMIZATION_LEVEL3
				#endif
				DXC_ARG_WARNINGS_ARE_ERRORS,
				L"-Qstrip_reflect",	// Strip reflections onto separate blob
				L"-Qstrip_debug"	// Strip debug information onto separate blob
			};

			OutputDebugStringA("Compiling ");
			OutputDebugStringA(info.file);

			return compile(source_blob.Get(), args, _countof(args));
		}

		IDxcBlob* compile(IDxcBlobEncoding* source_blob, LPCWSTR* args, u32 num_args) {
			DxcBuffer buffer{};
			buffer.Encoding = DXC_CP_ACP;
			buffer.Ptr = source_blob->GetBufferPointer();
			buffer.Size = source_blob->GetBufferSize();

			HRESULT hr{ S_OK };
			ComPtr<IDxcResult> results{ nullptr };
			DXCall(hr = _compiler->Compile(&buffer, args, num_args, _include_handler.Get(), IID_PPV_ARGS(&results)));
			if (FAILED(hr)) return nullptr;

			ComPtr<IDxcBlobUtf8> errors{ nullptr };
			DXCall(hr = results->GetOutput(DXC_OUT_ERRORS, IID_PPV_ARGS(&errors), nullptr));
			if (FAILED(hr)) return nullptr;

			if (errors && errors->GetStringLength()) {
				OutputDebugStringA("\nShader compilation error: \n");
				OutputDebugStringA(errors->GetStringPointer());
			}
			else {
				OutputDebugStringA(" [ Succeeded ] ");
			}
			OutputDebugStringA("\n");

			HRESULT status{ S_OK };
			DXCall(hr = results->GetStatus(&status));
			if (FAILED(hr) || FAILED(status)) return nullptr;

			ComPtr<IDxcBlob> shader{ nullptr };
			DXCall(hr = results->GetOutput(DXC_OUT_OBJECT, IID_PPV_ARGS(&shader), nullptr));
			if (FAILED(hr)) return nullptr;

			return shader.Detach();
		}

	private:
		constexpr static const char* _profile_strings[]{ "vs_6_5", "hs_6_5", "ds_6_5", "gs_6_5", "ps_6_5", "cs_6_5", "as_6_5", "ms_6_5" };
		static_assert(_countof(_profile_strings) == ShaderType::count);

		ComPtr<IDxcCompiler3> _compiler{ nullptr };
		ComPtr<IDxcUtils> _utils{ nullptr };
		ComPtr<IDxcIncludeHandler> _include_handler{ nullptr };
	};

	decltype(auto) get_engine_shaders_path() {
		return std::filesystem::path{ graphics::get_engine_shaders_path(graphics::GraphicsPlatform::DIRECT3D12) };
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
			full_path = path;
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
			file.write((char*)byte_code.pShaderBytecode, byte_code.BytecodeLength);
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

	ShaderCompiler compiler{};

	for (u32 i{ 0 }; i < EngineShader::count; ++i) {
		auto& info = shader_files[i];
		path = shaders_source_path;
		path += info.file;
		full_path = std::filesystem::absolute(path);

		if (!std::filesystem::exists(full_path)) return false;

		ComPtr<IDxcBlob> compiled_shader{ compiler.compile(info, full_path) };

		if (compiled_shader && compiled_shader->GetBufferPointer() && compiled_shader->GetBufferSize()) {
			shaders.emplace_back(std::move(compiled_shader));
		}
		else return false;
	}

	return save_compiled_shaders(shaders);
}