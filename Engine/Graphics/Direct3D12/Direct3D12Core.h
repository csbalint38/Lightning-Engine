#pragma once
#include "Direct3D12CommonHeaders.h"

namespace lightning::graphics::direct3d12 {
	struct D3D12FrameInfo {
		u32 surface_width{};
		u32 surface_height{};
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

	 [[nodiscard]] id3d12_device* const device();
	 [[nodiscard]] DescriptorHeap& rtv_heap();
	 [[nodiscard]] DescriptorHeap& dsv_heap();
	 [[nodiscard]] DescriptorHeap& srv_heap();
	 [[nodiscard]] DescriptorHeap& uav_heap();
	 [[nodiscard]] ConstantBuffer& c_buffer();
	 [[nodiscard]] u32 current_frame_index();
	void set_deferred_release_flag();

	[[nodiscard]] Surface create_surface(platform::Window window);
	void remove_surface(surface_id id);
	void resize_surface(surface_id id, u32 width, u32 height);
	[[nodiscard]] u32 surface_width(surface_id id);
	[[nodiscard]] u32 surface_height(surface_id id);
	void render_surface(surface_id id);
}