#include "Common.h"
#include "CommonHeaders.h"
#include "Components/Script.h"
#include "Graphics/Renderer.h"
#include "Platform/PlatformTypes.h"
#include "Platform/Platform.h"
#include "Content/ContentToEngine.h"
#include "ShaderCompilation.h"
#include "../ContentTools/ToolsCommon.h"
#include "Utilities/IOStream.h"

#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif

#include <Windows.h>
#include <atlsafe.h>

using namespace lightning;

namespace {
	HMODULE game_code_dll{ nullptr };
	using _get_script_creator = lightning::script::detail::script_creator(*)(size_t);
	_get_script_creator script_creator{ nullptr };
	using _get_script_names = LPSAFEARRAY(*)(void);
	_get_script_names script_names{ nullptr };

	util::vector<graphics::RenderSurface> surfaces;

	struct ShaderData {
		u32 type;
		u32 code_size;
		u32 byte_code_size;
		u32 errors_size;
		u32 assembly_size;
		u32 hash_size;
		u8* code;
		u8* byte_code_error_assembly_hash;
		const char* function_name;
		const char* extra_args;
	};

	struct ShaderGroupData {
		u32 type;
		u32 count;
		u32 data_size;
		u8* data;
	};

	struct EngineInitError {
		enum ErrorCode : u32 {
			SUCCEEDED = 0,
			UNKNOWN,
			SHADER_COMPILATION,
			GRAPHICS
		};
	};

	u8* patch_material_data(u8* data) {
		util::BlobStreamReader blob{ data };
		const u32 texture_count{ blob.read<u32>() };

		if (texture_count) {
			id::id_type* const texture_ids{ (id::id_type* const)blob.position() };
			*reinterpret_cast<id::id_type**>(const_cast<u8*>(blob.position())) = texture_ids;
		}

		return (u8*)blob.position();
	}
}

EDITOR_INTERFACE EngineInitError::ErrorCode initialize_engine() {
	while (!compile_shaders()) {
		if (MessageBox(nullptr, "Failed to compile engine shaders.", "Shader Compilation Error", MB_RETRYCANCEL) != IDRETRY) {
			return EngineInitError::SHADER_COMPILATION;
		}
	}

	return graphics::initialize(graphics::GraphicsPlatform::DIRECT3D12) ? EngineInitError::SUCCEEDED : EngineInitError::GRAPHICS;
}

EDITOR_INTERFACE void shutdown_engine() { graphics::shutdown(); }

EDITOR_INTERFACE u32 load_game_code_dll(const char* dll_path) {
	if (game_code_dll) return FALSE;
	game_code_dll = LoadLibraryA(dll_path);
	assert(game_code_dll);

	script_creator = (_get_script_creator)GetProcAddress(game_code_dll, "get_script_creator_from_engine");
	script_names = (_get_script_names)GetProcAddress(game_code_dll, "get_script_names_from_engine");

	assert(script_creator);
	assert(script_names);

	return (game_code_dll && script_creator && script_names) ? TRUE : FALSE;
}

EDITOR_INTERFACE u32 unload_game_code_dll() {
	if (!game_code_dll) return FALSE;
	assert(game_code_dll);
	[[maybe_unused]] int result{ FreeLibrary(game_code_dll) };
	assert(result);
	game_code_dll = nullptr;
	return TRUE;
}

EDITOR_INTERFACE script::detail::script_creator get_script_creator(const char* name) {
	return (game_code_dll && script_creator) ? script_creator(script::detail::string_hash()(name)) : nullptr;
}

EDITOR_INTERFACE LPSAFEARRAY get_script_names() {
	return (game_code_dll && script_names) ? script_names() : nullptr;
}

EDITOR_INTERFACE u32 create_renderer_surface(HWND host, s32 width, s32 height) {
	assert(host);
	platform::WindowInitInfo info{ nullptr, host, nullptr, 0, 0, width, height };
	graphics::RenderSurface surface{ platform::create_window(&info), {} };
	assert(surface.window.is_valid());
	surfaces.emplace_back(surface);
	return (u32)surfaces.size() - 1;
}

EDITOR_INTERFACE void remove_renderer_surface(u32 id) {
	assert(id < surfaces.size());
	platform::remove_window(surfaces[id].window.get_id());
}

EDITOR_INTERFACE HWND get_window_handle(u32 id) {
	assert(id < surfaces.size());
	return (HWND)surfaces[id].window.handle();
}

EDITOR_INTERFACE void resize_renderer_surface(u32 id) {
	assert(id < surfaces.size());
	surfaces[id].window.resize(0, 0);
}

EDITOR_INTERFACE id::id_type add_shader_group(ShaderGroupData* data) {
	assert(data && data->type < graphics::ShaderType::count && data->count && data->data_size && data->data);

	const u32 count{ data->count };

	util::BlobStreamReader blob{ data->data };
	const u32* const keys{ (const u32*)blob.position() };

	blob.skip(count * sizeof(u32));

	const u8** shader_pointers{ (const u8**)alloca(count * sizeof(u8*)) };

	for (u32 i{ 0 }; i < count; ++i) {
		const u32 block_size{ sizeof(u64) + content::CompiledShader::hash_length + *(u32*)blob.position() };
		shader_pointers[i] = blob.position();
		blob.skip(block_size);
	}

	assert(blob.position() == (data->data + data->data_size));

	return content::add_shader_group(shader_pointers, count, keys);
}

EDITOR_INTERFACE void remove_shader_group(id::id_type id) {
	content::remove_shader_group(id);
}

EDITOR_INTERFACE u32 compile_shader(ShaderData* data) {
	assert(data && data->code && data->code_size && data->function_name);

	ShaderFileInfo info{};
	info.function = data->function_name;
	info.type = (graphics::ShaderType::Type)data->type;

	util::vector<std::string> extra_args{ split(data->extra_args, ';') };
	util::vector<std::wstring> w_extra_args{};

	for (const auto& str : extra_args) {
		w_extra_args.emplace_back(to_wstring(str.c_str()));
	}

	std::unique_ptr<u8[]> compiled_shader{ compile_shader(info, data->code, data->code_size, w_extra_args, true) };

	if (!compiled_shader) return FALSE;

	u64 buffer_size{ 0 };

	{
		util::BlobStreamReader blob{ compiled_shader.get() };
		data->byte_code_size = (u32)blob.read<u64>();
		data->hash_size = content::CompiledShader::hash_length;
		blob.skip(data->hash_size + data->byte_code_size);
		data->errors_size = (u32)blob.read<u64>();
		data->assembly_size = (u32)blob.read<u64>();
		buffer_size = data->byte_code_size + data->hash_size + data->errors_size + data->assembly_size;
	}

	assert(buffer_size);

	data->byte_code_error_assembly_hash = (u8*)CoTaskMemAlloc(buffer_size);

	assert(data->byte_code_error_assembly_hash);

	{
		util::BlobStreamReader blob{ compiled_shader.get() };
		blob.skip(sizeof(u64));
		blob.read(&data->byte_code_error_assembly_hash[buffer_size - data->hash_size], data->hash_size);
		blob.read(data->byte_code_error_assembly_hash, data->byte_code_size);
		blob.skip(2 * sizeof(u64));
		blob.read(&data->byte_code_error_assembly_hash[data->byte_code_size], data->errors_size + data->assembly_size);
	}
}

EDITOR_INTERFACE id::id_type create_resource(u8* data, content::AssetType::Type type) {
	if (type == content::AssetType::MATERIAL) {
		data = patch_material_data(data);
	}

	assert(data && type < content::AssetType::count);

	return content::create_resource(data, type);
}

EDITOR_INTERFACE void destroy_resource(id::id_type id, content::AssetType::Type type) {
	assert(id::is_valid(id) && type < content::AssetType::count);

	content::destroy_resource(id, type);
}