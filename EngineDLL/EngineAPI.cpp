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
#include "Components/Entity.h"
#include "Components/Geometry.h"
#include "Components/Transform.h"
#include "Utilities/Threading.h"

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

	struct ViewportSurface : public graphics::RenderSurface {
		graphics::Camera camera{};
		util::vector<id::id_type> geometry_ids{};
	};

	util::vector<ViewportSurface> surfaces;
	util::vector<u8> frame_info_buffer;

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
			// *reinterpret_cast<id::id_type**>(const_cast<u8*>(blob.position())) = texture_ids;
			blob.skip(sizeof(id::id_type) * texture_count);
			*((id::id_type**)blob.position()) = texture_ids;
		}

		return (u8*)blob.position();
	}

	void calculate_thresholds(const game_entity::entity_id* const entity_ids, f32* const thresholds, u32 count, u32 surface_id) {
		game_entity::Entity camera{ game_entity::entity_id{surfaces[surface_id].camera.entity_id()} };

		using namespace DirectX;

		math::v3 pos{ camera.position() };
		XMVECTOR camera_pos{ XMLoadFloat3(&pos) };

		for (u32 i{ 0 }; i < count; ++i) {
			assert(id::is_valid(entity_ids[i]));

			pos = game_entity::Entity{ entity_ids[i] }.position();
			XMVECTOR entity_pos{ XMLoadFloat3(&pos) };
			XMVECTOR distance{ XMVector3LengthEst(camera_pos - entity_pos) };
			XMStoreFloat(&thresholds[i], distance);
		}
	}

	// TEMP:
	graphics::Light lights[4]{};

	math::v3 rgb_to_color(u8 r, u8 g, u8 b) { return { r / 255.f,g / 255.f,b / 255.f }; }

	game_entity::Entity create_one_game_entity(math::v3 position, math::v3 rotation, geometry::InitInfo* geometry_info, const char* script_name, math::v3 scale = { 1.f,1.f,1.f }) {
		transform::InitInfo transform_info{};
		DirectX::XMVECTOR quat{ DirectX::XMQuaternionRotationRollPitchYawFromVector(DirectX::XMLoadFloat3(&rotation)) };
		math::v4a rot_quat;
		DirectX::XMStoreFloat4A(&rot_quat, quat);
		memcpy(&transform_info.rotation[0], &rot_quat.x, sizeof(transform_info.rotation));
		memcpy(&transform_info.position[0], &position.x, sizeof(transform_info.position));
		memcpy(&transform_info.scale[0], &scale.x, sizeof(transform_info.scale));

		script::InitInfo script_info{};

		if (script_name) {
			script_info.script_creator = script::detail::get_script_creator_from_engine(script::detail::string_hash()(script_name));

			assert(script_info.script_creator);
		}

		game_entity::EntityInfo entity_info{};
		entity_info.transform = &transform_info;
		entity_info.script = &script_info;
		entity_info.geometry = geometry_info;

		game_entity::Entity entity{ game_entity::create(entity_info) };

		assert(entity.is_valid());

		return entity;
	}

	graphics::Camera create_camera() {
		game_entity::Entity entity{ create_one_game_entity({ 0, 1, 10 }, { 0, -math::PI, 0 }, nullptr, nullptr) };

		return graphics::create_camera(graphics::PerspectiveCameraInitInfo{ entity.get_id() });
	}

	void remove_camera(graphics::Camera& camera) {
		const game_entity::entity_id id{ camera.entity_id() };

		graphics::remove_camera(camera.get_id());

		camera = {};

		game_entity::remove(id);
	}

	void create_lights() {
		graphics::create_light_set(0);
		
		graphics::LightInitInfo info{};

		info.entity_id = create_one_game_entity({}, { .23f, 5.28f, 0.f }, nullptr, nullptr).get_id();
		info.type = graphics::Light::DIRECTIONAL;
		info.light_set_key = 0;
		info.intensity = .5f;
		info.color = rgb_to_color(174, 174, 174);
		lights[0] = graphics::create_light(info);

		info.entity_id = create_one_game_entity({}, { .25f, 5.28f - math::PI, 0.f }, nullptr, nullptr).get_id();
		info.intensity = 1.f;
		lights[1] = graphics::create_light(info);
		
		info.entity_id = create_one_game_entity({}, { math::PI * .5f, 0.f, 0.f }, nullptr, nullptr).get_id();
		info.color = rgb_to_color(17, 27, 48);
		info.intensity = .5f;
		lights[2] = graphics::create_light(info);

		info.entity_id = create_one_game_entity({}, { -math::PI * .5f, 0, 0 }, nullptr, nullptr).get_id();
		info.color = rgb_to_color(63, 47, 30);
		lights[3] = graphics::create_light(info);
	}

	void remove_lights() {
		for (u32 i{ 0 }; i < _countof(lights); ++i) {
			if (!lights[i].is_valid()) continue;

			const game_entity::entity_id id{ lights[i].entity_id() };

			graphics::remove_light(lights[i].get_id(), lights[i].light_set_key());
			game_entity::remove(id);
		}

		graphics::remove_light_set(0);
	}
	// ENDTEMP
}

extern util::TicketMutex mutex;

math::v4 to_quat(math::v3 angles, bool is_degrees);

EDITOR_INTERFACE EngineInitError::ErrorCode initialize_engine() {
	while (!compile_shaders()) {
		if (MessageBox(nullptr, "Failed to compile engine shaders.", "Shader Compilation Error", MB_RETRYCANCEL) != IDRETRY) {
			return EngineInitError::SHADER_COMPILATION;
		}
	}

	return graphics::initialize(graphics::GraphicsPlatform::DIRECT3D12) ? EngineInitError::SUCCEEDED : EngineInitError::GRAPHICS;
}

EDITOR_INTERFACE void shutdown_engine() {
	// TEMP
	if (lights[0].is_valid()) remove_lights();
	// ENDTEMP

	graphics::shutdown();
}

EDITOR_INTERFACE u32 load_game_code_dll(const char* dll_path) {
	if (game_code_dll) return FALSE;
	game_code_dll = LoadLibraryA(dll_path);
	assert(game_code_dll);

	if (!game_code_dll) [[unlikely]] return FALSE;

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
	std::lock_guard lock{ mutex };

	// TEMP
	if (!lights[0].is_valid()) create_lights();
	// ENDTEMP

	assert(host);
	platform::WindowInitInfo info{ nullptr, host, nullptr, 0, 0, width, height };
	ViewportSurface surface{};

	surface.window = platform::create_window(&info);
	surface.surface = graphics::create_surface(surface.window);
	surface.camera = create_camera();

	assert(surface.window.is_valid());
	surfaces.emplace_back(surface);
	return (u32)surfaces.size() - 1;
}

EDITOR_INTERFACE void remove_renderer_surface(u32 id) {
	std::lock_guard lock{ mutex };

	assert(id < surfaces.size());

	remove_camera(surfaces[id].camera);
	graphics::remove_surface(surfaces[id].surface.get_id());
	platform::remove_window(surfaces[id].window.get_id());
}

EDITOR_INTERFACE HWND get_window_handle(u32 id) {
	std::lock_guard lock{ mutex };

	assert(id < surfaces.size());
	return (HWND)surfaces[id].window.handle();
}

EDITOR_INTERFACE void resize_renderer_surface(u32 id) {
	std::lock_guard lock{ mutex };

	assert(id < surfaces.size());

	platform::Window& window{ surfaces[id].window };

	window.resize(0, 0);
	surfaces[id].surface.resize(window.width(), window.height());
	surfaces[id].camera.aspect_ratio((f32)window.width() / window.height());
}

EDITOR_INTERFACE id::id_type add_shader_group(ShaderGroupData* data) {
	assert(data && data->type < graphics::ShaderType::count && data->count && data->data_size && data->data);

	const u32 count{ data->count };

	util::BlobStreamReader blob{ data->data };
	const u32* const keys{ (const u32*)blob.position() };

	blob.skip(count * sizeof(u32));

	const u8** shader_pointers{ (const u8**)_malloca(count * sizeof(u8*)) };

	if (!shader_pointers) return id::invalid_id;

	for (u32 i{ 0 }; i < count; ++i) {
		const u32 block_size{ sizeof(u64) + content::CompiledShader::hash_length + *(u32*)blob.position() };
		shader_pointers[i] = blob.position();
		blob.skip(block_size);
	}

	assert(blob.position() == (data->data + data->data_size));

	id::id_type group_id = content::add_shader_group(shader_pointers, count, keys);
	_freea(shader_pointers);
	
	return group_id;
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

	if (!data->byte_code_error_assembly_hash) [[unlikely]] return FALSE;

	assert(data->byte_code_error_assembly_hash);

	{
		util::BlobStreamReader blob{ compiled_shader.get() };
		blob.skip(sizeof(u64));
		blob.read(&data->byte_code_error_assembly_hash[buffer_size - data->hash_size], data->hash_size);
		blob.read(data->byte_code_error_assembly_hash, data->byte_code_size);
		blob.skip(2 * sizeof(u64));
		blob.read(&data->byte_code_error_assembly_hash[data->byte_code_size], data->errors_size + data->assembly_size);
	}

	return TRUE;
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

EDITOR_INTERFACE void set_geometry_ids(u32 surface_id, id::id_type* geometry_ids, u32 count) {
	std::lock_guard lock{ mutex };

	assert(surface_id < surfaces.size());

	ViewportSurface& surface{ surfaces[surface_id] };

	surface.geometry_ids.resize(count);

	if (count) {
		memcpy(surface.geometry_ids.data(), geometry_ids, count * sizeof(id::id_type));
	}
}

EDITOR_INTERFACE void render_frame(u32 surface_id, id::id_type camera_id, u64 light_set) {
	std::lock_guard lock{ mutex };

	assert(surface_id < surfaces.size());

	// TEMP
	light_set = 0;
	// ENDTEMP

	const ViewportSurface& surface{ surfaces[surface_id] };

	const u32 count{ (u32)surface.geometry_ids.size() };
	const u64 item_id_buffer_size{ sizeof(id::id_type) * count };
	const u64 threshold_buffer_size{ sizeof(f32) * count };
	const u64 entity_id_buffer_size{ sizeof(game_entity::entity_id) * count };

	frame_info_buffer.resize(item_id_buffer_size + threshold_buffer_size + entity_id_buffer_size);

	id::id_type* item_ids{ (id::id_type*)frame_info_buffer.data() };
	f32* thresholds{ (f32*)(frame_info_buffer.data() + item_id_buffer_size) };
	game_entity::entity_id* entity_ids{ (game_entity::entity_id*)(frame_info_buffer.data() + item_id_buffer_size + threshold_buffer_size) };

	if (count) {
		const id::id_type* const geometry_ids{ surface.geometry_ids.data() };

		geometry::get_entity_ids(geometry_ids, entity_ids, count);
		geometry::get_render_item_ids(surface.geometry_ids.data(), item_ids, count);
		calculate_thresholds(entity_ids, thresholds, count, surface_id);
	}

	graphics::FrameInfo info{};
	info.render_item_ids = item_ids;
	info.render_item_count = count;
	info.thresholds = thresholds;
	info.camera_id = !id::is_valid(camera_id) ? surface.camera.get_id() : graphics::camera_id{ camera_id };
	info.light_set_key = light_set;

	surface.surface.render(info);
}