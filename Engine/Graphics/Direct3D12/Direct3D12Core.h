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

	ID3D12Device10* const device();
}