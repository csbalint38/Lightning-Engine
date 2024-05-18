#include "Direct3D12Core.h"

namespace lightning::graphics::direct3d12::core {
	namespace {
		ID3D12Device10* main_device;
		IDXGIFactory7* dxgi_factory;
	}

	bool initialize() {
		if (main_device) shutdown();

		u32 dxgi_factory_flags{ 0 };

		#ifdef _DEBUG
		dxgi_factory_flags |= DXGI_CREATE_FACTORY_DEBUG;
		#endif

		CreateDXGIFactory2(dxgi_factory_flags, IID_PPV_ARGS(&dxgi_factory));
	}
}