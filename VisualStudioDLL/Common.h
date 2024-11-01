#pragma once
#ifndef EDITOR_INTERFACE
#define EDITOR_INTERFACE extern "C" __declspec(dllexport)
#endif

#define WIN32_LEAN_AND_MEAN
#include <Windows.h>

extern CRITICAL_SECTION cs;