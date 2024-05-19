#include "..\Engine\Platform\Platform.h"
#include "..\Engine\Platform\PlatformTypes.h"
#include "..\Graphics\Renderer.h"
#include "TestRenderer.h"

#if TEST_RENDERER
	using namespace lightning;

	graphics::RenderSurface _surfaces[4];

	LRESULT win_proc(HWND hwnd, UINT msg, WPARAM wparam, LPARAM lparam) {
		switch (msg) {
		case WM_DESTROY: {
			bool all_closed{ true };
			for (u32 i{ 0 }; i < _countof(_surfaces); ++i) {
				if (!_surfaces[i].window.is_closed()) {
					all_closed = false;
				}
			}
			if (all_closed) {
				PostQuitMessage(0);
				return 0;
			}
			break;
		}
		case WM_SYSCHAR:
			if (wparam == VK_RETURN && (HIWORD(lparam) & KF_ALTDOWN)) {
				platform::Window win{ platform::window_id{(id::id_type)GetWindowLongPtrW(hwnd, GWLP_USERDATA)} };
				win.set_fullscreen(!win.is_fullscreen());
				return 0;
			}
			break;
		}
		return DefWindowProcW(hwnd, msg, wparam, lparam);
	}

	void create_render_surface(graphics::RenderSurface& surface, platform::WindowInitInfo info) {
		surface.window = platform::create_window(&info);
	}

	void destroy_render_surface(graphics::RenderSurface& surface) {
		platform::remove_window(surface.window.get_id());
	}

	bool EngineTest::initialize() {
		bool result { graphics::initialize(graphics::GraphicsPlatform::DIRECT3D12) };
		if (!result) return result;
		platform::WindowInitInfo info[]{
			{&win_proc, nullptr, L"TestWindow1", 100, 100, 800, 800},
			{&win_proc, nullptr, L"TestWindow2", 150, 150, 400, 800},
			{&win_proc, nullptr, L"TestWindow3", 200, 200, 800, 400},
			{&win_proc, nullptr, L"TestWindow4", 250, 250, 400, 400}
		};
		static_assert(_countof(_surfaces) == _countof(info));

		for (u32 i{ 0 }; i < _countof(_surfaces); ++i) {
			create_render_surface(_surfaces[i], info[i]);
		}
		return result;
	}

	void EngineTest::run() {
		std::this_thread::sleep_for(std::chrono::milliseconds(10));
		graphics::render();
	}

	void EngineTest::shutdown() {
		for (u32 i{ 0 }; i < _countof(_surfaces); ++i) {
			destroy_render_surface(_surfaces[i]);
		}
		graphics::shutdown();
	}
#endif