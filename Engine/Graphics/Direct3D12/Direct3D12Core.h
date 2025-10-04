#pragma once
#include "Direct3D12CommonHeaders.h"

namespace lightning::graphics::direct3d12 {
	namespace camera { class D3D12Camera; }

	struct D3D12FrameInfo {
		const FrameInfo* info{ nullptr };
		camera::D3D12Camera* camera{ nullptr };
		D3D12_GPU_VIRTUAL_ADDRESS global_shader_data{ 0 };
		u32 surface_width{ 0 };
		u32 surface_height{ 0 };
		id::id_type light_culling_id{ id::invalid_id };
		u32 frame_index{ 0 };
	};
}

namespace lightning::graphics::direct3d12::core {
	bool initialize();
	void shutdown();

	template<typename T> constexpr void release(T*& resource) {
		if (resource) {
			resource->Release();
			resource = nullptr;
		}
	}

	namespace detail {
		void deferred_release(IUnknown* resource);
	}

	template<typename T> constexpr void deferred_release(T*& resource) {
		if (resource) {
			detail::deferred_release(resource);
			resource = nullptr;
		}
	}

	void set_option(RendererOptions::Option option, const void* const parameter, u32 parameter_size);
	void get_option(RendererOptions::Option option, void* const parameter, u32 parameter_size);

	[[nodiscard]] id3d12_device* device();
	[[nodiscard]] DescriptorHeap& rtv_heap();
	[[nodiscard]] DescriptorHeap& dsv_heap();
	[[nodiscard]] DescriptorHeap& srv_heap();
	[[nodiscard]] DescriptorHeap& uav_heap();
	[[nodiscard]] ConstantBuffer& c_buffer();
	[[nodiscard]] u32 current_frame_index();
	void set_deferred_release_flag(u32 frame_index);

	[[nodiscard]] bool allow_tearing();
	[[nodiscard]] bool vsync_enabled();
	[[nodiscard]] Surface create_surface(platform::Window window);
	void remove_surface(surface_id id);
	void resize_surface(surface_id id, u32 width, u32 height);
	[[nodiscard]] u32 surface_width(surface_id id);
	[[nodiscard]] u32 surface_height(surface_id id);
	void render_surface(surface_id id, FrameInfo info);
	void capture_surface(surface_id id, const wchar_t* path);
}