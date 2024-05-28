#include "..\Engine\Platform\Platform.h"
#include "..\Engine\Platform\PlatformTypes.h"
#include "..\Graphics\Renderer.h"
#include "TestRenderer.h"
#include "ShaderCompilation.h"

#if TEST_RENDERER
	using namespace lightning;

	graphics::RenderSurface _surfaces[4];
	TimeIt timer;
	void destroy_render_surface(graphics::RenderSurface& surface);

	LRESULT win_proc(HWND hwnd, UINT msg, WPARAM wparam, LPARAM lparam) {
		switch (msg) {
			case WM_DESTROY: {
				bool all_closed{ true };
				for (u32 i{ 0 }; i < _countof(_surfaces); ++i) {
					if (_surfaces[i].window.is_valid()) {
						if (_surfaces[i].window.is_closed()) {
							destroy_render_surface(_surfaces[i]);
						}
						else {
							all_closed = false;
						}
					}
				}
				if (all_closed) {
					PostQuitMessage(0);
					return 0;
				}
			}
			break;
			case WM_SYSCHAR:
				if (wparam == VK_RETURN && (HIWORD(lparam) & KF_ALTDOWN)) {
					platform::Window win{ platform::window_id{(id::id_type)GetWindowLongPtrW(hwnd, GWLP_USERDATA)} };
					win.set_fullscreen(!win.is_fullscreen());
					return 0;
				}
				break;
			case WM_KEYDOWN:
				if (wparam == VK_ESCAPE) {
					PostMessage(hwnd, WM_CLOSE, 0, 0);
					return 0;
				}
		}
		return DefWindowProc(hwnd, msg, wparam, lparam);
	}

	void create_render_surface(graphics::RenderSurface& surface, platform::WindowInitInfo info) {
		surface.window = platform::create_window(&info);
		surface.surface = graphics::create_surface(surface.window);
	}

	void destroy_render_surface(graphics::RenderSurface& surface) {
		graphics::RenderSurface temp{ surface };
		surface = {};
		if (temp.surface.is_valid()) graphics::remove_surface(temp.surface.get_id());
		if (temp.window.is_valid()) platform::remove_window(temp.window.get_id());
	}

	bool EngineTest::initialize() {

		while (!comple_shaders()) {
			if (MessageBox(nullptr, "Failed to compile engine shaders", "Shader Compilation Error", MB_RETRYCANCEL) != IDRETRY) {
				return false;
			}
		}

		if(!graphics::initialize(graphics::GraphicsPlatform::DIRECT3D12)) return false;

		platform::WindowInitInfo info[]{
			{&win_proc, nullptr, L"TestWindow1", 100, 100, 800, 400},
			{&win_proc, nullptr, L"TestWindow2", 100, 100, 800, 400},
			{&win_proc, nullptr, L"TestWindow3", 100, 100, 800, 400},
			{&win_proc, nullptr, L"TestWindow4", 100, 100, 800, 400}
		};
		static_assert(_countof(_surfaces) == _countof(info));

		for (u32 i{ 0 }; i < _countof(_surfaces); ++i) {
			create_render_surface(_surfaces[i], info[i]);
		}
		return true;
	}

	void EngineTest::run() {
		timer.begin();
		std::this_thread::sleep_for(std::chrono::milliseconds(10));
		for (u32 i{ 0 }; i < _countof(_surfaces); ++i) {
			if (_surfaces[i].surface.is_valid()) {
				_surfaces[i].surface.render();
			}
		}
		timer.end();
	}

	void EngineTest::shutdown() {
		for (u32 i{ 0 }; i < _countof(_surfaces); ++i) {
			destroy_render_surface(_surfaces[i]);
		}
		graphics::shutdown();
	}
#endif