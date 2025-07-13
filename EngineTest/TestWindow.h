#pragma once

#include "Test.h"
#include "..\Engine\Platform\Platform.h"
#include "..\Engine\Platform\PlatformTypes.h"

using namespace lightning;

platform::Window _windows[4];

#ifdef _WIN64
LRESULT win_proc(HWND hwnd, UINT msg, WPARAM wparam, LPARAM lparam) {
	switch (msg) {
		case WM_DESTROY: {
			bool all_closed{ true };
			for (u32 i{ 0 }; i < _countof(_windows); ++i) {
				if (!_windows[i].is_closed()) {
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
#endif