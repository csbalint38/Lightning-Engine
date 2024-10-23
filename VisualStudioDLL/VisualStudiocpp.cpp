#include "Common.h"

#include <atlbase.h>
#include <atlcom.h>
#include <oleauto.h>
#include <shlwapi.h>

#import "libid:80CC9F66-E7D8-4DDD-85B6-D9E6CD0E93E2" auto_rename

const wchar_t* vs_name = L"VisualStudio.DTE.17.0";
CComPtr<EnvDTE::_DTE> vs_instance{ nullptr };

EDITOR_INTERFACE void __stdcall open_visual_studio(const wchar_t* solution_path) {
	CComPtr<IRunningObjectTable> rot;
	CComPtr<IEnumMoniker> enum_moniker;
	CComPtr<IMoniker> moniker;

	if (vs_instance != nullptr) {
		return;
	}

	HRESULT hr = GetRunningObjectTable(0, &rot);

	if (FAILED(hr)) {
		return;
	}

	hr = rot->EnumRunning(&enum_moniker);

	if (FAILED(hr)) {
		return;
	}

	while (enum_moniker->Next(1, &moniker, nullptr) == S_OK) {
		CComPtr<IBindCtx> bind_ctx;
		hr = CreateBindCtx(0, &bind_ctx);

		if (FAILED(hr)) {
			continue;
		}

		LPOLESTR display_name{ nullptr };
		hr = moniker->GetDisplayName(bind_ctx, nullptr, &display_name);
		
		if (SUCCEEDED(hr) && wcsstr(display_name, vs_name) != nullptr) {
			CComPtr<IUnknown> p_unk{ nullptr };
			hr = rot->GetObject(moniker, &p_unk);

			if (SUCCEEDED(hr)) {
				p_unk->QueryInterface(__uuidof(EnvDTE::_DTE), (void**)&vs_instance);
				
				if (vs_instance != nullptr) {
					CComPtr<EnvDTE::_Solution> solution;
					vs_instance->get_Solution(&solution);

					if (solution != nullptr) {
						BSTR opened_solution_path;
						solution->get_FullName(&opened_solution_path);

						if (wcscmp(opened_solution_path, solution_path) == 0) {
							vs_instance->MainWindow->Activate();
							vs_instance->MainWindow->Visible = true;
							break;
						}
					}
				}
			}
		}
		moniker.Release();
		CoTaskMemFree(display_name);
		vs_instance.Release();
	}
	enum_moniker.Release();
	rot.Release();
	
	if (vs_instance == nullptr) {
		CLSID clsid;
		HRESULT hr = CLSIDFromProgID(vs_name, &clsid);
		CComPtr<IUnknown> p_unk{ nullptr };

		if (FAILED(hr)) {
			return;
		}

		hr = CoCreateInstance(clsid, nullptr, CLSCTX_LOCAL_SERVER, IID_IUnknown, (void**)&p_unk);

		if (FAILED(hr)) {
			return;
		}

		hr = p_unk->QueryInterface(__uuidof(EnvDTE::_DTE), (void**)&vs_instance);

		if (FAILED(hr)) {
			return;
		}

		CComPtr<EnvDTE::_Solution> solution;
		vs_instance->get_Solution(&solution);
		solution->Open((_bstr_t)solution_path);

		vs_instance->MainWindow->Activate();
		vs_instance->MainWindow->Visible = true;
	}
}

EDITOR_INTERFACE void __stdcall close_visual_studio() {
	if (vs_instance != nullptr) {
		CComPtr<EnvDTE::_Solution> solution;
		vs_instance->get_Solution(&solution);

		if (solution->GetIsOpen() == VARIANT_TRUE) {
			vs_instance->ExecuteCommand((_bstr_t)"File.SaveAll", (_bstr_t)"");
			solution->Close(VARIANT_TRUE);
		}

		vs_instance->Quit();
	}
}

EDITOR_INTERFACE bool add_files(const wchar_t* solution, const wchar_t* project_name, const wchar_t** files, int files_count) {
	open_visual_studio(solution);

	if (!vs_instance->Solution->IsOpen) {
		vs_instance->Solution->Open(solution);
	}
	else {
		vs_instance->ExecuteCommand((_bstr_t)"File.SaveAll", (_bstr_t)"");
	}
	
	for (int i = 1; i < vs_instance->Solution->Projects->Count + 1; i++) {
		CComPtr<EnvDTE::Project> project;
		project = vs_instance->Solution->Item(i);

		const wchar_t* source_file{ nullptr };

		if (wcsstr((const wchar_t*)project->UniqueName, project_name) != nullptr) {
			for (int j = 0; j < files_count; j++) {
				project->ProjectItems->AddFromFile(files[j]);

				if (wcsstr(files[j], (_bstr_t)".cpp")) {
					source_file = files[j];
				}
			}

			if (source_file != nullptr) {
				vs_instance->ItemOperations->OpenFile(source_file, EnvDTE::vsViewKindTextView)->Visible = true;
			}
			vs_instance->MainWindow->Activate();
			vs_instance->MainWindow->Visible = true;
		}
	}
	return true;
}
