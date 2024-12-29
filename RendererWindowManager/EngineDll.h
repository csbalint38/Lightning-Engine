#pragma once

#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif // !WIN32_LEAN_AND_MEAN

#ifndef EDITOR_IMPORT
#define EDITOR_IMPORT extern "C" __declspec(dllimport)
#endif // !EDITOR_IMPORT


#include <Windows.h>

EDITOR_IMPORT unsigned int __cdecl create_renderer_surface(HWND parent, int widt, int height);
EDITOR_IMPORT void __cdecl remove_renderer_surface(unsigned int surface_id);
EDITOR_IMPORT HWND __cdecl get_window_handle(unsigned int surface_id);