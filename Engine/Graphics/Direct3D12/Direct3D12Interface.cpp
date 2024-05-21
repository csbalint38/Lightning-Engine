#include "CommonHeaders.h"
#include "Direct3D12Interface.h"
#include "Direct3d12Core.h"
#include "Graphics\GraphicsPlatformInterface.h"

namespace lightning::graphics::direct3d12 {

	void get_platform_interface(PlatformInterface& pi) {
		pi.initialize = core::initialize;
		pi.shutdown = core::shutdown;

		pi.surface.create = core::create_surface;
		pi.surface.remove = core::remove_surface;
		pi.surface.resize = core::resize_surface;
		pi.surface.width = core::surface_width;
		pi.surface.height = core::surface_height;
		pi.surface.render = core::render_surface;
	}
}