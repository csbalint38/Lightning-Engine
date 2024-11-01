// dllmain.cpp : Defines the entry point for the DLL application.
#include "Common.h"

#include <comdef.h>
#include <crtdbg.h>

CRITICAL_SECTION cs;

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        CoInitializeEx(NULL, COINIT_MULTITHREADED);
        InitializeCriticalSection(&cs);
        #if _DEBUG
            _CrtSetDbgFlag(_CRTDBG_ALLOC_MEM_DF | _CRTDBG_LEAK_CHECK_DF);
        #endif
        break;
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        DeleteCriticalSection(&cs);
        CoUninitialize();
        break;
    }
    return TRUE;
}

