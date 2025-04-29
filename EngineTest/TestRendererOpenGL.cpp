// This file will be "merged" with generic TestRenderer.cpp after the OpenGL implementation cautches up with DirectX12.

#include "../Platform/PlatformTypes.h"
#include "../Platform/Platform.h"
#include "../Graphics/Renderer.h"
#include "TestRenderer.h"

#ifdef OPENGL
using namespace lightning;

graphics::RenderSurface _surfaces[4];

LRESULT win_proc(HWND hwnd, UINT msg, WPARAM wparam, LPARAM lparam) {
    switch(msg) {
        case WM_DESTROY: {
            bool all_closed{ true };

            for(u32 i{ 0 }; i < _countof(_surfaces); ++i)
                if(!_surfaces[i].window.is_closed()) all_closed = false;

            if(all_closed) {
                PostQuitMessage(0);

                return 0;
            }
        }
        break;
        case WM_SYSCHAR:
            if(wparam == VK_RETURN && (HIWORD(lparam) & KF_ALTDOWN)) {
                platform::Window win{ platform::window_id{ (id::id_type)GetWindowLongPtr(hwnd, GWLP_USERDATA) } };
                win.set_fullscreen(!win.is_fullscreen());

                return 0;
            }
            break;
    }

    return DefWindowProc(hwnd, msg, wparam, lparam);
}

void create_render_surface(graphics::RenderSurface& surface, platform::WindowInitInfo& info) {
    surface.window = platform::create_window(&info);
}

void destroy_render_surface(graphics::RenderSurface& surface) {
    platform::remove_window(surface.window.get_id());
}

bool EngineTest::initialize() {
    bool result { graphics::initialize(graphics::GraphicsPlatform::OPEN_GL) };

    if(!result) return result;

    platform::WindowInitInfo info[] {
        { &win_proc, nullptr, L"Test Window 1", 0, 0, 800, 600 },
        { &win_proc, nullptr, L"Test Window 2", 400, 300, 1200, 900 },
        { &win_proc, nullptr, L"Test Window 3", 600, 450, 1400, 1050 },
        { &win_proc, nullptr, L"Test Window 4", 700, 525, 1500, 1125 }
    };

    static_assert(_countof(info) == _countof(_surfaces));

    for (u32 i{ 0 }; i < _countof(_surfaces); ++i) 
        create_render_surface(_surfaces[i], info[i]);

    return result;
}

void EngineTest::run() {
    std::this_thread::sleep_for(std::chrono::milliseconds(10));
}

void EngineTest::shutdown() {
    for(u32 i{ 0 }; i < _countof(_surfaces); ++i)
        destroy_render_surface(_surfaces[i]);

    graphics::shutdown();
}
#endif // OPENGL