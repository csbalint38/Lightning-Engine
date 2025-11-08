#include "Test.h"

#pragma comment(lib, "engine.lib")

#if TEST_ENTITY_COMPONENTS
#include "TestEntityComponents.h"
#elif TEST_WINDOW
#include "TestWindow.h"
#elif TEST_RENDERER
#include "TestRenderer.h"
#else
#error One of the tests need to be enabled
#endif

#ifdef _WIN64
#include <Windows.h>
#include <filesystem>

std::filesystem::path set_current_directory_to_executable_path() {
	wchar_t path[MAX_PATH]{};
	const uint32_t length{ GetModuleFileNameW(0, &path[0], MAX_PATH) };
	if (!length || GetLastError() == ERROR_INSUFFICIENT_BUFFER) return {};
	std::filesystem::path p{ path };
	std::filesystem::current_path(p.parent_path());

	return std::filesystem::current_path();
}

int WINAPI WinMain(_In_ HINSTANCE hInstance, _In_opt_ HINSTANCE hPrevInstance, _In_ LPSTR lpCmdLine,_In_ int nShowCmd) {
	#if _DEBUG
	_CrtSetDbgFlag(_CRTDBG_ALLOC_MEM_DF | _CRTDBG_LEAK_CHECK_DF);
	#endif

	set_current_directory_to_executable_path();

	EngineTest test{};
	if (test.initialize()) {
		MSG msg{};
		bool is_running{ true };
		while (is_running) {
			while (PeekMessage(&msg, NULL, 0, 0, PM_REMOVE)) {
				TranslateMessage(&msg);
				DispatchMessage(&msg);
				is_running &= (msg.message != WM_QUIT);
			}
			test.run();
		}
	}
	test.shutdown();
	return 0;
}

#elif __linux__
#include <X11/Xlib.h>

int main(int argc, char* argv[]) {
	XInitThreads();

	EngineTest test{};

	Display* display { XOpenDisplay(NULL) };

	if(display == NULL) return 1;

	Atom wm_delete_window = XInternAtom(display, "WM_DELETE_WINDOW", false);
	Atom quit_msg = XInternAtom(display, "QUIT_MSG", false);

	if(test.initialize(display)) {
		XEvent xev;
		bool is_running{ true };

		while(is_running) {
			if(XPending(display) > 0) {
				XNextEvent(display, &xev);

				switch(xev.type) {
					case KeyPress:
						break;
					case ClientMessage:
						if((Atom)xev.xclient.data.l[0] == wm_delete_window) {
							XPutBackEvent(display, &xev);
						}
						if((Atom)xev.xclient.data.l[0] == quit_msg) {
							is_running = false;
						}
						break;
				}
			}
			test.run(display);
		}

		test.shutdown();
		XCloseDisplay(display);

		return 0;
	}
}

#else
int main() {
	#if _DEBUG
	_CrtSetDbgFlag(_CRTDBG_ALLOC_MEM_DF | _CRTDBG_LEAK_CHECK_DF);
	#endif
	EngineTest test{};

	if (test.initialize()) {
		test.run();
	}

	test.shutdown();
}
#endif