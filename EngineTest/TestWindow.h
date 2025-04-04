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
#elif __linux__
enum Key {
	ENTER = 36
};

enum KeyState {
	ALT = 0X18
};

#endif

class EngineTest : Test {
	public:
		#ifdef _WIN64
		bool initialize() override {

			platform::WindowInitInfo info[]{
				{&win_proc, nullptr, L"TestWindow1", 100, 100, 800, 800},
				{&win_proc, nullptr, L"TestWindow2", 150, 150, 400, 800},
				{&win_proc, nullptr, L"TestWindow3", 200, 200, 800, 400},
				{&win_proc, nullptr, L"TestWindow4", 250, 250, 400, 400}
			};
			static_assert(_countof(_windows) == _countof(info));

			for (u32 i{ 0 }; i < _countof(_windows); ++i) {
				_windows[i] = platform::create_window(&info[i]);
			}
			return true;
		}

		void run() override {
			std::this_thread::sleep_for(std::chrono::milliseconds(10));
		}
		#elif __linux__
		bool Inititalize(void* disp) override {
			platform::WindwInitInfo info[]{
				{nullptr, nullptr, L"TestWindow1", 100, 100, 800, 800},
				{nullptr, nullptr, L"TestWindow2", 150, 150, 400, 800},
				{nullptr, nullptr, L"TestWindow3", 200, 200, 800, 400},
				{nullptr, nullptr, L"TestWindow4", 250, 250, 400, 400}
			};

			static_assert(_countof(_windows) == _countof(info));

			for (u32 i{ 0 }; i < _countof(_windows); ++i) {
				_windows[i] = platform::create_window(&info[i], disp);
			}

		}

		void run(void* disp) override {
			std::this_thread::sleep_for(std::chrono::milliseconds(10));

			Display* display { (Display*)disp };

			Window window = XCreateSimpleWindow(display, DefaultRootWindow(display), 0, 0, 100, 100, 0, 0, 0);
			Atom wm_delete_window = XInternAtom(display, "WM_DELETE_WINDOW", false);
			Atom quit_msg = XInternAtom(display, "QUIT_MSG", false);

			XEvent xev;

			if(XPending(display) > 0) {
				XNextEvent(display, &xev);

				switch(xev.type) {
					case ConfigureNotify:
						XConfigureEvent xce{ xev.xconfigure };

						for(u32 i{ 0 }; i< _countof(_windows); ++i) {
							if(*((Window*)_windows[i].handle()) == xenv.xany.window) {
								if((u32)xce.width != windows[i].width() || (u32)xce.height != _windows[i].height()) {
									_windows[i].resize((u32)xce.width, (u32)xce.height);
								}
							}
						}
						break;
					
					case ClientMessage:
						if((Atom)xev.xclient.data.l[0] == wm_delete_window) {
							for(u32 i{ 0 }; i < _countof(_windows); ++i) {
								if(*((Window*)_windows[i].handle()) == xev.xany.window) {
									_windows[i].close();
								}
							}

							bool all_closed{ true };

							for(u32 i{ 0 }; i < _countof(_windows); i++) {
								if (!_windows[i].is_closed()) {
									all_closed = false;
									break;
								}
							}

							if(all_closed) {
								XEvent close;
								
								close.xclient.type = ClientMessage;
								close.xclient.serial = window;
								close.xclient.send_event = true;
								close.xclient.message_type = XinternAtom(display, "QUIT_MSG", false);
								close.xclient.format = 32;
								close.xclient.window = 0;
								close.xclient.data.l[0] = XInternAtom(display, "QUIT_MSG", false);

								XSendEvent(display, window, false, NoEventMask, &close);
							}
						}

						else {
							XPutBackEvent(display, &xev);
						}

						break;
					
					case KeyPress:
						if(xev.xkey.state == State::ALT && xev.xkey.keycode == Key::ENTER) {
							for(u32 i { 0 }; i < _countof(_windows); i++) {
								if(*((Window*)_windows[i].handle()) == xev.xany.window) {
									_windows[i].set_fullscreen(!_windows[i].is_fullscreen());
								}
							}
						}
				}
			}
		}
		#endif

		void shutdown() override {
			for (u32 i{ 0 }; i < _countof(_windows); ++i) {
				platform::remove_window(_windows[i].get_id());
			}
		}
};
