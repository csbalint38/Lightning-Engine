#pragma once
#include "Direct3D12CommonHeaders.h"

namespace lightning::graphics::direct3d12::d3dx {
	constexpr struct {
		D3D12_HEAP_PROPERTIES default_heap{
			D3D12_HEAP_TYPE_DEFAULT,
			D3D12_CPU_PAGE_PROPERTY_UNKNOWN,
			D3D12_MEMORY_POOL_UNKNOWN,
			0,
			0
		};
	} heap_properties;
}