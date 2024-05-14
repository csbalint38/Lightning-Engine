#include "Platform.h"
#include "PlatformTypes.h"

namespace lightning::platform {
	#ifdef _WIN64

	namespace {

		struct WindowInfo {
			HWND hwnd{ nullptr };
			RECT client_area{ 0, 0, 1920, 1080 };
			RECT fullcsreen_area{};
			POINT top_left{ 0, 0 };
			DWORD style{ WS_VISIBLE };
			bool is_fullscreen{ false };
			bool is_closed{ false };
		};

		LRESULT CALLBACK internal_window_proc(HWND hwnd, UINT msg, WPARAM wparam, LPARAM lparam) {
			LONG_PTR long_ptr{ GetWindowLongPtr(hwnd, 0) };
			return long_ptr ? (hwnd, msg, wparam, lparam) : DefWindowProc(hwnd, msg, wparam, lparam);
		}
	}

	Window create_window(const WindowInitInfo* const init_info) {
		window_proc callback{ init_info ? init_info->callback : nullptr };
		window_handle parent{ init_info ? init_info->parent : nullptr };

		WNDCLASSEXW wc;
		ZeroMemory(&wc, sizeof(wc));
		wc.cbSize = sizeof(WNDCLASSEX);
		wc.style = CS_HREDRAW | CS_VREDRAW;
		wc.lpfnWndProc = internal_window_proc;
		wc.cbClsExtra = 0;
		wc.cbWndExtra = callback ? sizeof(callback) : 0;
		wc.hInstance = 0;
		wc.hIcon = LoadIcon(NULL, IDI_APPLICATION);
		wc.hCursor = LoadCursor(NULL, IDC_ARROW);
		wc.hbrBackground = CreateSolidBrush(RGB(26, 48, 76));
		wc.lpszMenuName = NULL;
		wc.lpszClassName = L"LightningWindow";
		wc.hIconSm = LoadIcon(NULL, IDI_APPLICATION);

		RegisterClassExW(&wc);

		WindowInfo info{};
		RECT rc{ info.client_area };
		
		AdjustWindowRect(&rc, info.style, FALSE);

		const wchar_t* caption{ (init_info && init_info->caption) ? init_info->caption : L"Lightning Game" };
		const s32 left{ (init_info && init_info->left) ? init_info->left : info.client_area.left };
		const s32 top{ (init_info && init_info->top) ? init_info->top : info.client_area.top };
		const s32 width{ (init_info && init_info->width) ? init_info->width : rc.right - rc.left };
		const s32 height{ (init_info && init_info->height) ? init_info->height : rc.bottom - rc.top };

		info.style |= parent ? WS_CHILD : WS_OVERLAPPEDWINDOW;
		info.hwnd = CreateWindowExW(0, wc.lpszClassName, caption, info.style, left, top, width, height, parent, NULL, NULL, NULL);

		if (info.hwnd) {}

		return {};
	}

	#elif
	#error "Must implement at least one platform"
	#endif
}