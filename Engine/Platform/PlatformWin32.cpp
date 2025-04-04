#include "Platform.h"
#include "PlatformTypes.h"
#include "Input/InputWin32.h"

namespace lightning::platform {
	#ifdef _WIN64

	namespace {

		struct WindowInfo {
			HWND hwnd{ nullptr };
			RECT client_area{ 0, 0, 1920, 1080 };
			RECT fullscreen_area{};
			POINT top_left{ 0, 0 };
			DWORD style{ WS_VISIBLE };
			bool is_fullscreen{ false };
			bool is_closed{ false };
		};

		util::free_list<WindowInfo> windows;

		WindowInfo& get_from_id(window_id id) {
			assert(windows[id].hwnd);
			return windows[id];
		}

		WindowInfo& get_from_handle(window_handle handle) {
			const window_id id{ (id::id_type)GetWindowLongPtrW(handle, GWLP_USERDATA) };
			return get_from_id(id);
		}

		bool resized{ false };
		LRESULT CALLBACK internal_window_proc(HWND hwnd, UINT msg, WPARAM wparam, LPARAM lparam) {
			switch (msg) {
				case WM_NCCREATE: {
					DEBUG_OP(SetLastError(0));
					const window_id id{ windows.add() };
					windows[id].hwnd = hwnd;
					SetWindowLongPtr(hwnd, GWLP_USERDATA, (LONG_PTR)id);
					assert(GetLastError() == 0);
				}
				break;
				case WM_DESTROY:
					get_from_handle(hwnd).is_closed = true;
					break;
				case WM_SIZE:
					resized = (wparam != SIZE_MINIMIZED);
					break;
				default:
					break;
			}

			input::process_input_message(hwnd, msg, wparam, lparam);

			if (resized && GetKeyState(VK_LBUTTON) >= 0) {
				WindowInfo& info{ get_from_handle(hwnd) };
				assert(info.hwnd);
				GetClientRect(info.hwnd, info.is_fullscreen ? &info.fullscreen_area : &info.client_area);
				resized = false;
			}

			if (msg == WM_SYSCOMMAND && wparam == SC_KEYMENU) {
				return 0;
			}

			LONG_PTR long_ptr{ GetWindowLongPtr(hwnd, 0) };
			return long_ptr ? ((window_proc)long_ptr)(hwnd, msg, wparam, lparam) : DefWindowProc(hwnd, msg, wparam, lparam);
		}

		void resize_window(const WindowInfo& info, const RECT& area) {
			RECT window_rect{ area };
			AdjustWindowRect(&window_rect, info.style, FALSE);

			const s32 width{ window_rect.right - window_rect.left };
			const s32 height{ window_rect.bottom - window_rect.top };

			MoveWindow(info.hwnd, info.top_left.x, info.top_left.y, width, height, true);
		}

		void resize_window(window_id id, u32 width, u32 height) {
			WindowInfo& info{ get_from_id(id) };

			if (info.style & WS_CHILD) {
				GetClientRect(info.hwnd, &info.client_area);
			}
			else {
				RECT& area{ info.is_fullscreen ? info.fullscreen_area : info.client_area };
				area.bottom = area.top + height;
				area.left = area.right + width;

				resize_window(info, area);
			}
		}

		void set_window_fullscreen(window_id id, bool is_fullscreen) {
			WindowInfo& info{ get_from_id(id) };

			if (info.is_fullscreen != is_fullscreen) {
				info.is_fullscreen = is_fullscreen;
			}

			if (is_fullscreen) {
				GetClientRect(info.hwnd, &info.client_area);
				RECT rect;
				GetWindowRect(info.hwnd, &rect);
				info.top_left.x = rect.left;
				info.top_left.y = rect.top;
				SetWindowLongPtrW(info.hwnd, GWL_STYLE, 0);
				ShowWindow(info.hwnd, SW_MAXIMIZE);
			}
			else {
				SetWindowLongPtrW(info.hwnd, GWL_STYLE, info.style);
				resize_window(info, info.client_area);
				ShowWindow(info.hwnd, SW_SHOWNORMAL);
			}
		}
	}

	static bool is_window_fullscreen(window_id id) {
		return get_from_id(id).is_fullscreen;
	}

	static window_handle get_window_handle(window_id id) {
		return get_from_id(id).hwnd;
	}

	static void set_window_caption(window_id id, const wchar_t* caption) {
		WindowInfo& info{ get_from_id(id) };
		SetWindowTextW(info.hwnd, caption);
	}

	static math::u32v4 get_window_size(window_id id) {
		WindowInfo& info{ get_from_id(id) };
		RECT& area{ info.is_fullscreen ? info.fullscreen_area : info.client_area };

		return { (u32)area.left, (u32)area.top, (u32)area.right, (u32)area.bottom };
	}

	static bool is_window_closed(window_id id) {
		return get_from_id(id).is_closed;
	}

	Window create_window(const WindowInitInfo* init_info) {
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

		RegisterClassEx(&wc);

		WindowInfo info{};
		info.client_area.right = (init_info && init_info->width) ? info.client_area.left + init_info->width : info.client_area.right;
		info.client_area.bottom = (init_info && init_info->height) ? info.client_area.top + init_info->height : info.client_area.bottom;
		info.style |= parent ? WS_CHILD : WS_OVERLAPPEDWINDOW;
		RECT rect{ info.client_area };
		
		AdjustWindowRect(&rect, info.style, FALSE);

		const wchar_t* caption{ (init_info && init_info->caption) ? init_info->caption : L"Lightning Game" };
		const s32 left{ init_info ? init_info->left : info.top_left.x};
		const s32 top{ init_info ? init_info->top : info.top_left.y };
		const s32 width{ rect.right - rect.left };
		const s32 height{ rect.bottom - rect.top };

		info.hwnd = CreateWindowEx(0, wc.lpszClassName, caption, info.style, left, top, width, height, parent, NULL, NULL, NULL);

		if (info.hwnd) {

			DEBUG_OP(SetLastError(0));
			if (callback) SetWindowLongPtrW(info.hwnd, 0, (LONG_PTR)callback);
			assert(GetLastError() == 0);

			ShowWindow(info.hwnd, SW_SHOWNORMAL);
			UpdateWindow(info.hwnd);

			window_id id{ (id::id_type)GetWindowLongPtr(info.hwnd, GWLP_USERDATA) };
			windows[id] = info;

			return Window{ id };
		}

		return {};
	}

	void remove_window(window_id id) {
		WindowInfo& info{ get_from_id(id) };
		DestroyWindow(info.hwnd);
		windows.remove(id);
	}
}
#include "IncludeWindowCpp.h"
//#endif

#elif __APPLE__
#elif __linux__
namespace lightning::platform {
	namespace {
		struct WindowInfo {
			Window wnd{};
			Display* display{ nullptr };
			s32 left;
			s32 top;
			s32 width;
			s32 height;
			bool is_fullscreen{ false };
			bool is_closed{ false };
		};

		util::free_list<WindowInfo> windows;

		windowInfo& get_from_id (window_id) {
			assert(windows[id].wnd);

			return windows[id];
		}

		void resize window(window_id id, u32 width, u32 height) {
			WindowInfo& info { get_from_id(id) };

			info.width = width;
			info.height = height;

			XClearWindow(info.display, info.wnd);
		}

		void set_window_fullscreen(window_id id, bool is_fullscreen) {
			WindowInfo& info { get_from_id(id) };

			if(info.is_fullscreen != is_fullscreen) {
				info.is_fullscreen = is_fullscreen;

				if(is_fullscreen) {
					XEvent xev;
					Atom wm_state{ XInternAtom(info.display, "_NET_WM_STATE", false) };
					Atom fullscreen{ XInternAtom(info.display, "_NET_WM_STATE_FULLSCREEN", false) };
					memset(&xev, 0, sizeof(xev));
					xev.type = ClientMessage;
					xev.xclient.window = info.wnd;
					xev.xclient.message_type = wm_state;
					xev.xclient.format = 32;
					xev.xclient.data.l[0] = 1;
					xev.xclient.data.l[1] = fullscreen;
					xev.xclient.data.l[2] = 0;

					XSendEvent(info.display, DefaultRootWindoe(info.display), false, SubstructureNotifyMask | SubstructureRedirectMask, &xev);
				}
				else {
					XEvent xev;
					Atom wm_state{ XInternAtom(info.display, "_NET_WM_STATE", false) };
					Atom fullscreen{ XInternAtom(info.display, "_NET_WM_STATE_FULLSCREEN", false) };
					memset(&xev, 0, sizeof(xev));
					xev.type = ClientMessage;
					xev.xclient.window = info.wnd;
					xev.xclient.message_type = wm_state;
					xev.xclient.format = 32;
					xev.xclient.data.l[0] = 0;
					xev.xclient.data.l[1] = fullscreen;
					xev.xclient.data.l[2] = 0;

					XSendEvent(info.display, DefaultRootWindow(info.display), false, SubstructureNotifyMask | SubstructureRedirectMask, &xev);
				}
			}
		}

		bool is_window_fullscreen(window_id id) {
			return get_from_id(id).is_fullscreen;
		}

		window_handle get_window_handle(window_id, id) {
			return &get_from_id(id).wnd;
		}

		Display* get_display(window_id id) {
			return get_from_id(id).display;
		}

		void set_window_caption(window_id id, cont wchar_t* caption) {
			WindowInfo& info { get_from_id(id) };
			size_t out_size = (sizeof(caption) * sizeof(wchar_t)) + 1;
			char title[out_size];
			wcstombs(title, caption, out_size);
			XStoreName(info.display, info.wnd, title);
		}

		math::u32v4 get_window_size(window_id id) {
			WindowInfo& info { get_from_id(id) };

			return { (u32)info.left, (u32)info.top, (u32)info.width - (u32)info.left, (u32)info.height - (u32)info.top };
		}

		bool is_window_closed(window_id id) {
			return get_from_id(id).is_closed;
		}

		void set_window_closed(window_id id) {
			WindowInfo& info { get_from_id(id) };
			get_from_id(id).is_closed = true;
			XDestroyWindow(info.display, info.wnd);
		}
	}

	Window create_window(const WindowInitInfo* const init_info, void* disp) {
		Display* display{ (Display*)disp };
		
		window_handle parent{ init_infi ? init_info->parent : &(DefaultRootWindow(display)) };

		if(parent == nullptr) {
			parent = &(DefaultRootWindow(display));
		}
		
		assert(parent != nullptr);

		int screen(DefaultScreen(display));
		Visual* visual{ DefaultVisual(display, screen) };
		Colormap colormap{ XCreateColormap(display, DefaultRootWindow(display), visual, AllocNone) };

		XSetWindowAttributes attributes;
		attributes.event_mask = ExposureMask | KeyPressMask | KeyReleaseMask | StructureNotifyMask | ButtonPressMask | ButtonReleaseMask | PointerMotionMask;
		attributes.colormap = colormap;
		
		WindowInfo& info{};
		info.left = (init_info && init_info->left) ? init_info->left : 0;
		info.top = (init_info && init_info->top) ? init_info->top : 0;
		info.width = (init_info && init_info->width) ? init_info->width : DisplayWidth(display, DefaultScreen(display));
		info.height = (init_info && init_info->height) ? init_info->height : DisplayHeight(display, DefaultScreen(display));
		info.display = display;

		const wchar_t* caption{ (init_info && init_info->caption) ? init_info->caption : L"Lightning Game" };
		size_t out_size = (sizeof(caption) * sizeof(wchar_t)) + 1;
		char title[out_size];
		wcstombs(title, caption, out_size);

		Window wnd{ XCreateWindow(display, *parent, info.left, info.top, info.width, info.height, 0, DefaultDepth(display, screen), InputOutput, visual, CWColorMap | CWEventMask, &attributes) };
		info.wnd = wnd;

		Atom wm_delete_window = XInternAtom(display, "WM_DELETE_WINDOW", false);
		XSetWMProtocols(display, wnd, &wm_delete_window, 1);

		XMapWindow(display, wnd);
		XStoreName(display, wnd, title);

		const window_id id { windows.add(info) };

		return window{ id };
	}

	void remove_window(window_id id) {
		WindowInfo& info{ get_from_id(id) };
		get_from_id(id).is_closed = true;
		XDestroyWindow(info.display, info.wnd);
		windows.remove(id);
	}
}
#endif

void Window::close() {
	set_window_closed(m_id);
}