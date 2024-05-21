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

	void render() {
		gfx.render();
	}

	Surface create_surface(platform::Window window) {
		return gfx.surface.create(window);
	}

	void remove_surface(surface_id id) {
		assert(id::is_valid(id));
		gfx.surface.remove(id);
	}

	void Surface::resize(u32 width, u32 height) const {
		assert(is_valid());
		gfx.surface.resize(_id, width, height);
	}

	u32 Surface::width() const {
		assert(is_valid());
		return gfx.surface.width(_id);
	}

	u32 Surface::height() const {
		assert(is_valid());
		return gfx.surface.height(_id);
	}

	void Surface::render() const {
		assert(is_valid());
		gfx.surface.render(_id);
	}
}