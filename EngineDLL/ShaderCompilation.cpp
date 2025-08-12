#include "ShaderCompilation.h"

#include <wrl.h>
#include <dxcapi.h>
#include <d3d12shader.h>

#include "Graphics/Renderer.h"
#include "Content/ContentToEngine.h"
#include "Utilities/IOStream.h"

#include <fstream>
#include <filesystem>

using namespace lightning;
using namespace Microsoft::WRL;

#ifdef _DEBUG
#ifndef DXCall
#define DXCall(x)							\
if(FAILED(x)) {								\
	char line_number[32];					\
	sprintf_s(line_number, "%u", __LINE__);	\
	OutputDebugStringA("Error in: ");		\
	OutputDebugStringA(__FILE__);			\
	OutputDebugStringA("\nline: ");			\
	OutputDebugStringA(line_number);		\
	OutputDebugStringA("\n");				\
	OutputDebugStringA(#x);					\
	OutputDebugStringA("\n");				\
	__debugbreak();							\
}
#endif
#else
#ifndef DXCall
#define DXCall(x) x
#endif
#endif

namespace {
	#ifdef _DEBUG
	constexpr const char* shaders_source_path{ "../../Engine/Graphics/Direct3D12/Shaders/" };
	#else
	constexpr const char* shaders_source_path{ "Engine/Graphics/Direct3D12/Shaders/" };
	#endif

	struct EngineShader {
		enum Id : u32 {
			FULLSCREEN_TRIANGLE_VS = 0,
			POST_PROCESS_PS,
			GRID_FRUSTUMS_CS,
			LIGHT_CULLING_CS,

			count
		};
	};

	struct EngineShaderInfo {
		EngineShader::Id id;
		ShaderFileInfo info;
	};


	constexpr EngineShaderInfo engine_shader_files[]{
		{
			EngineShader::FULLSCREEN_TRIANGLE_VS,
			{ "FullscreenTriangle.hlsl", "fullscreen_triangle_vs", graphics::ShaderType::VERTEX },
		},
		{
			EngineShader::POST_PROCESS_PS,
			{ "PostProcess.hlsl", "post_process_ps", graphics::ShaderType::PIXEL },
		},
		{
			EngineShader::GRID_FRUSTUMS_CS,
			{ "GridFrustums.hlsl", "compute_grid_frustum_cs", graphics::ShaderType::COMPUTE },
		},
		{
			EngineShader::LIGHT_CULLING_CS,
			{ "CullLights.hlsl", "cull_lights_cs", graphics::ShaderType::COMPUTE },
		}
	};

	std::wstring to_wstring(const char* c) {
		std::string s{ c };
		return { s.begin(), s.end() };
	}

	static_assert(_countof(engine_shader_files) == EngineShader::count);

	struct DxcCompiledShader {
		ComPtr<IDxcBlob> byte_code;
		ComPtr<IDxcBlobUtf8> errors;
		ComPtr<IDxcBlobUtf8> assembly;
		DxcShaderHash hash;
	};

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

		DxcCompiledShader compile(u8* data, u32 data_size, graphics::ShaderType::Type type, const char* function, util::vector<std::wstring>& extra_args) {
			assert(_compiler && _utils && _include_handler);
			assert(data && data_size && function);
			assert(type < graphics::ShaderType::count);

			HRESULT hr{ S_OK };

			ComPtr<IDxcBlobEncoding> source_blob{ nullptr };
			DXCall(hr = _utils->CreateBlob(data, data_size, 0, &source_blob));

			if (FAILED(hr)) return {};

			assert(source_blob && source_blob->GetBufferSize());

			ShaderFileInfo info{};
			info.function = function;
			info.type = type;

			OutputDebugStringA("Compiling");
			OutputDebugStringA(function);
			OutputDebugStringA("\n");

			return compile(source_blob.Get(), get_args(info, extra_args));
		}

		DxcCompiledShader compile(ShaderFileInfo info, std::filesystem::path full_path, util::vector<std::wstring>& extra_args) {
			assert(_compiler && _utils && _include_handler);
			HRESULT hr{ S_OK };

			ComPtr<IDxcBlobEncoding> source_blob{ nullptr };
			DXCall(hr = _utils->LoadFile(full_path.c_str(), nullptr, &source_blob));
			if (FAILED(hr)) return {};
			assert(source_blob && source_blob->GetBufferSize());

			OutputDebugStringA("Compiling ");
			OutputDebugStringA(info.file_name);
			OutputDebugStringA(": ");
			OutputDebugStringA(info.function);
			OutputDebugStringA("\n");

			return compile(source_blob.Get(), get_args(info, extra_args));
		}

		DxcCompiledShader compile(IDxcBlobEncoding* source_blob, util::vector<std::wstring> compiler_args) {
			DxcBuffer buffer{};
			buffer.Encoding = DXC_CP_ACP;
			buffer.Ptr = source_blob->GetBufferPointer();
			buffer.Size = source_blob->GetBufferSize();

			util::vector<LPCWSTR> args;
			for (const auto& arg : compiler_args) {
				args.emplace_back(arg.c_str());
			}

			HRESULT hr{ S_OK };
			ComPtr<IDxcResult> results{ nullptr };
			DXCall(hr = _compiler->Compile(&buffer, args.data(), (u32)args.size(), _include_handler.Get(), IID_PPV_ARGS(&results)));
			if (FAILED(hr)) return {};

			ComPtr<IDxcBlobUtf8> errors{ nullptr };
			DXCall(hr = results->GetOutput(DXC_OUT_ERRORS, IID_PPV_ARGS(&errors), nullptr));
			if (FAILED(hr)) return {};

			if (errors && errors->GetStringLength()) {
				OutputDebugStringA("\nShader compilation error: ");
				OutputDebugStringA(errors->GetStringPointer());
			}
			else {
				OutputDebugStringA(" [ Succeeded ] ");
			}
			OutputDebugStringA("\n");

			HRESULT status{ S_OK };
			DXCall(hr = results->GetStatus(&status));
			if (FAILED(hr) || FAILED(status)) return {};

			ComPtr<IDxcBlob> hash{ nullptr };
			DXCall(hr = results->GetOutput(DXC_OUT_SHADER_HASH, IID_PPV_ARGS(&hash), nullptr));
			if (FAILED(hr)) return {};
			DxcShaderHash* const hash_buffer{ (DxcShaderHash* const)hash->GetBufferPointer() };
			assert(!(hash_buffer->Flags & DXC_HASHFLAG_INCLUDES_SOURCE));
			OutputDebugStringA("Shader hash: ");
			for (u32 i{ 0 }; i < _countof(hash_buffer->HashDigest); ++i) {
				char hash_bytes[3]{};
				sprintf_s(hash_bytes, "%02x", (u32)hash_buffer->HashDigest[i]);
				OutputDebugStringA(hash_bytes);
				OutputDebugStringA(" ");
			}
			OutputDebugStringA("\n");

			ComPtr<IDxcBlob> shader{ nullptr };
			DXCall(hr = results->GetOutput(DXC_OUT_OBJECT, IID_PPV_ARGS(&shader), nullptr));
			if (FAILED(hr)) return {};

			buffer.Ptr = shader->GetBufferPointer();
			buffer.Size = shader->GetBufferSize();

			ComPtr<IDxcResult> disasm_results{ nullptr };
			DXCall(hr = _compiler->Disassemble(&buffer, IID_PPV_ARGS(&disasm_results)));
			
			ComPtr<IDxcBlobUtf8> disassembly{ nullptr };
			DXCall(hr = disasm_results->GetOutput(DXC_OUT_DISASSEMBLY, IID_PPV_ARGS(&disassembly), nullptr));

			DxcCompiledShader result{shader.Detach(), errors.Detach(), disassembly.Detach() };
			memcpy(&result.hash.HashDigest[0], &hash_buffer->HashDigest[0], _countof(hash_buffer->HashDigest));

			return result;
		}

	private:
		constexpr static const char* _profile_strings[]{ "vs_6_6", "hs_6_6", "ds_6_6", "gs_6_6", "ps_6_6", "cs_6_6", "as_6_6", "ms_6_6" };
		static_assert(_countof(_profile_strings) == graphics::ShaderType::count);

		ComPtr<IDxcCompiler3> _compiler{ nullptr };
		ComPtr<IDxcUtils> _utils{ nullptr };
		ComPtr<IDxcIncludeHandler> _include_handler{ nullptr };

		util::vector<std::wstring> get_args(const ShaderFileInfo& info, util::vector<std::wstring>& extra_args) {
			util::vector<std::wstring> args{};
			if (info.file_name) args.emplace_back(to_wstring(info.file_name));
			args.emplace_back(L"-E");
			args.emplace_back(to_wstring(info.function));
			args.emplace_back(L"-T");
			args.emplace_back(to_wstring(_profile_strings[(u32)info.type]));
			args.emplace_back(L"-I");
			args.emplace_back(to_wstring(shaders_source_path));
			args.emplace_back(L"-enable-16bit-types");
			args.emplace_back(DXC_ARG_ALL_RESOURCES_BOUND);
			#if _DEBUG
			args.emplace_back(DXC_ARG_DEBUG);
			args.emplace_back(DXC_ARG_SKIP_OPTIMIZATIONS);
			#else
			args.emplace_back(DXC_ARG_OPTIMIZATION_LEVEL3);
			#endif
			args.emplace_back(DXC_ARG_WARNINGS_ARE_ERRORS);
			args.emplace_back(L"-Qstrip_reflect");
			args.emplace_back(L"-Qstrip_debug");

			for (const auto& arg : extra_args) {
				args.emplace_back(arg.c_str());
			}

			return args;
		}
	};

	decltype(auto) get_engine_shaders_path() {
		return std::filesystem::path{ graphics::get_engine_shaders_path(graphics::GraphicsPlatform::DIRECT3D12) };
	}

	bool compiled_shaders_are_up_to_date() {
		auto engine_shaders_path = get_engine_shaders_path();
		if (!std::filesystem::exists(engine_shaders_path)) return false;
		auto shaders_compilation_time = std::filesystem::last_write_time(engine_shaders_path);

		for (const auto& entry : std::filesystem::directory_iterator{ shaders_source_path }) {
			if (entry.last_write_time() > shaders_compilation_time) return false;
		}

		return true;
	}

	bool save_compiled_shaders(util::vector<DxcCompiledShader>& shaders) {
		auto engine_shaders_path = get_engine_shaders_path();
		std::filesystem::create_directories(engine_shaders_path.parent_path());
		std::ofstream file(engine_shaders_path, std::ios::out | std::ios::binary);
		if (!file || !std::filesystem::exists(engine_shaders_path)) {
			file.close();
			return false;
		}

		for (const auto& shader : shaders) {
			void* const byte_code{ shader.byte_code->GetBufferPointer() };
			const u64 byte_code_length{ shader.byte_code->GetBufferSize() };
			file.write((char*)&byte_code_length, sizeof(byte_code_length));
			file.write((char*)&shader.hash.HashDigest[0], _countof(shader.hash.HashDigest));
			file.write((char*)byte_code, byte_code_length);
		}

		file.close();

		return true;
	}

	std::unique_ptr<u8[]> pack_compiled_shader(DxcCompiledShader compiled_shader, bool include_errors_and_disassembly) {
		if (compiled_shader.byte_code && compiled_shader.byte_code->GetBufferPointer() && compiled_shader.byte_code->GetBufferSize()) {
			static_assert(content::CompiledShader::hash_length == _countof(DxcShaderHash::HashDigest));
			const u64 extra_size{ include_errors_and_disassembly ? sizeof(u64) + sizeof(u64) + compiled_shader.errors->GetStringLength() + compiled_shader.assembly->GetStringLength() : 0 };
			const u64 buffer_size{ sizeof(u64) + content::CompiledShader::hash_length + compiled_shader.byte_code->GetBufferSize() + extra_size };
			std::unique_ptr<u8[]> buffer{ std::make_unique<u8[]>(buffer_size) };
			util::BlobStreamWriter blob{ buffer.get(), buffer_size };
			blob.write(compiled_shader.byte_code->GetBufferSize());
			blob.write(compiled_shader.hash.HashDigest, content::CompiledShader::hash_length);
			blob.write((u8*)compiled_shader.byte_code->GetBufferPointer(), compiled_shader.byte_code->GetBufferSize());

			if (include_errors_and_disassembly) {
				blob.write(compiled_shader.errors->GetStringLength());
				blob.write(compiled_shader.assembly->GetStringLength());
				blob.write(compiled_shader.errors->GetStringPointer(), compiled_shader.errors->GetStringLength());
				blob.write(compiled_shader.assembly->GetStringPointer(), compiled_shader.assembly->GetStringLength());
			}

			assert(blob.offset() == buffer_size);

			return buffer;
		}

		return {};
	}
}

std::unique_ptr<u8[]> compile_shader(ShaderFileInfo info, u8* code, u32 code_size, util::vector<std::wstring>& extra_args, bool include_errors_and_disassembly) {
	assert(!info.file_name && info.function && code && code_size);

	return pack_compiled_shader(ShaderCompiler{}.compile(code, code_size, info.type, info.function, extra_args), include_errors_and_disassembly);
}

std::unique_ptr<u8[]> compile_shader(ShaderFileInfo info, const char* file_path, util::vector<std::wstring>& extra_args, bool include_errors_and_disassembly) {
	std::filesystem::path full_path{ file_path };
	full_path += info.file_name;

	if (!std::filesystem::exists(full_path)) return {};

	return pack_compiled_shader(ShaderCompiler{}.compile(info, full_path, extra_args), include_errors_and_disassembly);
}

bool compile_shaders() {
	if (compiled_shaders_are_up_to_date()) return true;

	ShaderCompiler compiler{};
	util::vector<DxcCompiledShader> shaders;
	std::filesystem::path full_path{};

	for (u32 i{ 0 }; i < EngineShader::count; ++i) {
		auto& file = engine_shader_files[i];
		full_path = shaders_source_path;
		full_path += file.info.file_name;

		if (!std::filesystem::exists(full_path)) return false;
		util::vector<std::wstring> extra_args{};

		if (file.id == EngineShader::GRID_FRUSTUMS_CS || file.id == EngineShader::LIGHT_CULLING_CS) {
			extra_args.emplace_back(L"-D");
			extra_args.emplace_back(L"TILE_SIZE=32");
		}

		DxcCompiledShader compiled_shader{ compiler.compile(file.info, full_path, extra_args) };

		if (compiled_shader.byte_code && compiled_shader.byte_code->GetBufferPointer() && compiled_shader.byte_code->GetBufferSize()) {
			shaders.emplace_back(std::move(compiled_shader));
		}
		else return false;
	}

	return save_compiled_shaders(shaders);
}