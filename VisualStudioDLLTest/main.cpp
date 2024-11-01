#include <Windows.h>
#include <iostream>
#include <thread>
#include <chrono>
#include <vector>
#include <atlbase.h>
#include <atlcom.h>
#include <oleauto.h>
#include <shlwapi.h>

#import "libid:80CC9F66-E7D8-4DDD-85B6-D9E6CD0E93E2" auto_rename

typedef void(__stdcall* OpenVisualStudioFunc)(const wchar_t*);
typedef void(__stdcall* CloseVisualStudioFunc)();
typedef void(__stdcall* AddFilesFunc)(const wchar_t*, const wchar_t*, const wchar_t**, int);
typedef void(__stdcall* BuildSolutionFunc)(const wchar_t*, const wchar_t*, bool);
typedef bool(__stdcall* GetLastBuildInfoFunc)();

int main() {
    const wchar_t* dll_path = L"C:/Users/balin/Documents/Lightning-Engine/x64/DebugEditor/vsidll.dll";

    // Load the DLL
    HMODULE hModule = LoadLibraryW(dll_path);
    if (!hModule) {
        std::cerr << "Failed to load the DLL!" << std::endl;
        return 1;
    }

    // Get function pointers
    OpenVisualStudioFunc open_visual_studio = (OpenVisualStudioFunc)GetProcAddress(hModule, "open_visual_studio");
    CloseVisualStudioFunc close_visual_studio = (CloseVisualStudioFunc)GetProcAddress(hModule, "close_visual_studio");
    AddFilesFunc add_files = (AddFilesFunc)GetProcAddress(hModule, "add_files");
    BuildSolutionFunc build_solution = (BuildSolutionFunc)GetProcAddress(hModule, "build_solution");
    GetLastBuildInfoFunc get_last_build_info = (GetLastBuildInfoFunc)GetProcAddress(hModule, "get_last_build_info");

    if (!open_visual_studio || !close_visual_studio || !add_files || !build_solution || !get_last_build_info) {
        std::cerr << "Failed to retrieve function pointers!" << std::endl;
        FreeLibrary(hModule);
        return 1;
    }

    // Paths and configuration
    const wchar_t* solution_path = L"C:\\Users\\balin\\Documents\\LightningProjects\\NewProject\\NewProject.sln";
    const wchar_t* project = L"NewProject";
    const wchar_t* config = L"DebugEditor";
    std::vector<const wchar_t*> files{ L"C:/Users/balin/Documents/Lightning-Engine/VisualStudioDLLTest/example.cpp" };
    const wchar_t** file_array = files.data();

    // Opening Visual Studio and adding files
    open_visual_studio(solution_path);
    add_files(solution_path, project, file_array, static_cast<int>(files.size()));

    // Build the solution
    build_solution(solution_path, config, true);
    bool build_result = get_last_build_info();

    if (build_result) {
        std::wcout << L"Build succeeded!" << std::endl;
    }
    else {
        std::wcout << L"Build failed or is currently debugging." << std::endl;
    }

    std::wcout << L"Press Enter to close the program and Visual Studio..." << std::endl;
    std::cin.get();

    // Clean up
    close_visual_studio();
    FreeLibrary(hModule);
    return 0;
}
