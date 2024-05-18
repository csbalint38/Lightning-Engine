#pragma once
#include "Direct3D12CommonHeaders.h"

namespace lightning::graphics::direct3d12::core {
	bool initialize();
	void shutdown();

	template<typename T> constexpr void release(T*& resource) {
		if (resource) {
			resource->Release();
			resource = nullptr;
		}
	}
}