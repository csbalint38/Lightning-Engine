#pragma once
#include "Direct3D12CommonHeaders.h"

namespace lightning::graphics::direct3d12::core {
	bool initialize();
	void shutdown();
	void render();

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

	ID3D12Device10* const device();
	u32 current_frame_index();
	void set_deferred_release_flag();
}