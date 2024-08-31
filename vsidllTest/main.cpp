#include <windows.h>
#include <iostream>

typedef void(__stdcall* OpenVisualStudioFunc)(const char* solutionPath);
typedef void(__stdcall* CloseVisualStudioFunc)();

int main()
{
    HMODULE hModule = LoadLibrary(L"C:/Users/balin/Documents/Lightning-Engine/VisualStudio/bin/x64/Debug/net8.0-windows10.0.22621.0/win-x64/native/vsi.dll");
    if (hModule == NULL)
    {
        std::cerr << "Failed to load the DLL" << std::endl;
        return -1;
    }

    OpenVisualStudioFunc OpenVisualStudio = (OpenVisualStudioFunc)GetProcAddress(hModule, "OpenVisualStudio");
    CloseVisualStudioFunc CloseVisualStudio = (CloseVisualStudioFunc)GetProcAddress(hModule, "CloseVisualStudio");

    if (OpenVisualStudio == NULL || CloseVisualStudio == NULL)
    {
        std::cerr << "Failed to get the function pointers" << std::endl;
        FreeLibrary(hModule);
        return -1;
    }

    const char* solutionPath = "C:/Users/balin/Documents/Primal/Primal.sln";
    OpenVisualStudio(solutionPath);

    std::cout << "Visual Studio should be open now..." << std::endl;
    std::cin.get();

    CloseVisualStudio();

    std::cout << "Visual Studio should be closed now." << std::endl;

    FreeLibrary(hModule);

    return 0;
}
