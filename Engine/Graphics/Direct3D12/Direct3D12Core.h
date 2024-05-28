#pragma once
#include "Direct3D12CommonHeaders.h"

namespace lightning::graphics::direct3d12 {
	class DescriptorHeap;
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

	id3d12_device* const device();
	DescriptorHeap& rtv_heap();
	DescriptorHeap& dsv_heap();
	DescriptorHeap& srv_heap();
	DescriptorHeap& uav_heap();
	u32 current_frame_index();
	void set_deferred_release_flag();

	Surface create_surface(platform::Window window);
	void remove_surface(surface_id id);
	void resize_surface(surface_id id, u32 width, u32 height);
	u32 surface_width(surface_id id);
	u32 surface_height(surface_id id);
	void render_surface(surface_id id);
}