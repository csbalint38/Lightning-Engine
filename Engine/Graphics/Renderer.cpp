#include "Renderer.h"
#include "GraphicsPlatformInterface.h"
#include "Direct3D12/Direct3D12Interface.h"

namespace lightning::graphics {
	namespace {
		PlatformInterface gfx{};

		bool set_platform_interface(GraphicsPlatform platform) {
			switch (platform) {
				case lightning::graphics::GraphicsPlatform::DIRECT3D12:
					direct3d12::get_platform_interface(gfx);
					break;
				default:
					return false;
			}
			return true;
		}
	}

	bool initialize(GraphicsPlatform platform) {
		return set_platform_interface(platform) && gfx.initialize();
	}

	void shutdown() {
		gfx.shutdown();
	}
}