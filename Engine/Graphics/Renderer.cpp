#include "Renderer.h"
#include "GraphicsPlatformInterface.h"

namespace lightning::graphics {
	namespace {
		PlatformInterface gfx{};

		bool set_platform_interface(GraphicsPlatform platform) {
			switch (platform) {
				case lightning::graphics::GraphicsPlatform::DIRECT3D12:
					direct3d12::get_platform_interface(gfx);
			}
		}
	}

	bool initialize(GraphicsPlatform platform) {
		return set_platform_interface(platform);
	}

	void shutdown() {
		gfx.shutdown();
	}
}