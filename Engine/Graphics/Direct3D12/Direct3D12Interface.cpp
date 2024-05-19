#include "CommonHeaders.h"
#include "Direct3D12Interface.h"
#include "Direct3d12Core.h"
#include "Graphics\GraphicsPlatformInterface.h"

namespace lightning::graphics::direct3d12 {

	void get_platform_interface(PlatformInterface& pi) {
		pi.initialize = core::initialize;
		pi.shutdown = core::shutdown;
		pi.render = core::render;
	}
}