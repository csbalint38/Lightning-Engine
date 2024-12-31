#include "EngineDLL.h"

#include <assert.h>
#include <string>

HWND renderer_window{ nullptr };

static LRESULT CALLBACK win_proc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam) {
	switch (message) {
	case WM_DESTROY:
		PostQuitMessage(0);
		break;
	case WM_SIZE:
		if (renderer_window) {
			RECT rect;
			GetClientRect(hWnd, &rect);
			SetWindowPos(renderer_window, nullptr, 0, 0, rect.right - rect.left, rect.bottom - rect.top, SWP_NOZORDER | SWP_SHOWWINDOW);
		}
		break;
	}

	return DefWindowProc(hWnd, message, wParam, lParam);
}

int main(int argc, char* argv[]) {
	assert(argc >= 2);

	std::string class_param{ argv[1] };
	std::wstring class_name = std::wstring(class_param.begin(), class_param.end());

	WNDCLASS wc = { 0 };
	wc.lpfnWndProc = win_proc;
	wc.hInstance = GetModuleHandle(nullptr);
	wc.lpszClassName = class_name.c_str();
	RegisterClass(&wc);

	constexpr unsigned int default_width{ 1280 };
	constexpr unsigned int default_height{ 720 };

	HWND host_window{ CreateWindow(wc.lpszClassName, L"Renderer Window", WS_OVERLAPPEDWINDOW | WS_VISIBLE, CW_USEDEFAULT, CW_USEDEFAULT, default_width, default_height, nullptr, nullptr, wc.hInstance, nullptr) };

	assert(host_window);

	unsigned int surface_id{ create_renderer_surface(host_window, default_width, default_height) };

	assert(surface_id != -1);

	renderer_window = get_window_handle(surface_id);
	SetWindowPos(renderer_window, nullptr, 0, 0, default_width, default_height, SWP_NOZORDER | SWP_SHOWWINDOW);

	MSG msg = { 0 };

	while (GetMessage(&msg, nullptr, 0, 0)) {
		TranslateMessage(&msg);
		DispatchMessage(&msg);
	}

	remove_renderer_surface(surface_id);
	DestroyWindow(host_window);

	return 0;
}