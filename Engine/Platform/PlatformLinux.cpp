#include "Platform.h"
#include "PlatformTypes.h"

namespace lightning::platform {
    #ifdef __linux__
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

		WindowInfo& get_from_id (window_id) {
			assert(windows[id].wnd);

			return windows[id];
		}

		void resize_window(window_id id, u32 width, u32 height) {
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

					XSendEvent(info.display, DefaultRootWindow(info.display), false, SubstructureNotifyMask | SubstructureRedirectMask, &xev);
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

		window_handle get_window_handle(window_id id) {
			return &get_from_id(id).wnd;
		}

		Display* get_display(window_id id) {
			return get_from_id(id).display;
		}

		void set_window_caption(window_id id, const wchar_t* caption) {
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
		
		window_handle parent{ init_info ? init_info->parent : &(DefaultRootWindow(display)) };

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

    void Window::set_fullscreen(bool is_fullscreen) const {
        assert(is_valid());
        set_window_fullscreen(_id, is_fullscreen);
    }

    bool Window::is_fullscreen() const {
        assert(is_valid());

        return is_window_fullscreen(_id);
    }

    void* Window::handle() const {
        assert(is_valid());

        return get_window_handle(_id);
    }

    void Window::set_caption(const wchar_t* caption) const {
        assert(is_valid());

        set_window_caption(_id, caption);
    }

    math::u32v4 Window::size() const {
        assert(is_valid());

        return get_window_size(_id);
    }

    void Window::resize(u32 width, u32 height) const {
        assert(is_valid());

        resize_window(id_, width, height);
    }

    u32 Window::width() const {
        math::u32v4 s{ size() };

        return s.z - s.x;
    }

    u32 Window::height() const {
        math::u32v4 s{ size() };

        return s.w - s.y;
    }

    bool Window::is_closed() const {
        assert(is_valid());

        return is_window_closed(_id);
    }

    void Window::close() {
	    set_window_closed(_id);
    }
#endif
}