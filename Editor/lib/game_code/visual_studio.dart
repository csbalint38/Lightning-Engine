import 'dart:ffi';
import 'package:ffi/ffi.dart';
import 'package:win32/win32.dart';

class VisualStudio {
  static Pointer<COMObject> _vsInstance = nullptr;
  static final String _progId = "VisualStudio.DTE.17.0";
  static final ole32 = DynamicLibrary.open('ole32.dll');

  static final getRunningObjectTable = ole32.lookupFunction<
      Int32 Function(Uint32 reserved, Pointer<Pointer<COMObject>> pprot),
      int Function(int reserved,
          Pointer<Pointer<COMObject>> pprot)>('GetRunningObjectTable');
  static final createBindCtx = ole32.lookupFunction<
      Int32 Function(Uint32 reserved, Pointer<Pointer<COMObject>> ppbc),
      int Function(
          int reserved, Pointer<Pointer<COMObject>> ppbc)>('CreateBindCtx');

  static void openVisualStudio(String solutionPath) {
    final rot = calloc<Pointer<COMObject>>();
    final monikerTable = calloc<Pointer<COMObject>>();
    final bindCtx = calloc<Pointer<COMObject>>();

    int hResult = CoInitializeEx(nullptr, COINIT.COINIT_APARTMENTTHREADED);
    if (FAILED(hResult)) throw COMException(hResult);

    //try {
    /*if (_vsInstance == nullptr) {
      hResult = getRunningObjectTable(0, rot);
      if (FAILED(hResult))
        throw Exception(
            "GetRunningObjectTable() returned HRESULT: ${hResult.toRadixString(16)}");

      print(rot.address);

      final rotInstance = IRunningObjectTable(rot.cast());
      print(rotInstance.addRef());
      hResult = rotInstance.enumRunning(monikerTable);
      if (FAILED(hResult)) throw COMException(hResult);

      final monikerTableInstance = IEnumMoniker(monikerTable.cast());
      monikerTableInstance.reset();

      hResult = createBindCtx(0, bindCtx);
      if (hResult < 0 || bindCtx == nullptr)
        throw Exception(
            "CreateBindCtx() returned HRESULT: ${hResult.toRadixString(16)}");

      final bindCtxInstance = IBindCtx(bindCtx.value);
      final currentMoniker = calloc<Pointer<COMObject>>();*/
/*
      while (monikerTableInstance.next(1, currentMoniker, nullptr) == S_OK) {
        final moniker = IMoniker(currentMoniker.value);

        final displayNamePtr = calloc<Pointer<Utf16>>();
        moniker.getDisplayName(bindCtxInstance.ptr, nullptr, displayNamePtr);

        final displayName = displayNamePtr.value.toDartString();
        calloc.free(displayNamePtr);

        if (displayName.contains(_progId)) {
          final dte2Pointer = calloc<Pointer<COMObject>>();
          hResult = rotInstance.getObject(moniker.ptr, dte2Pointer);
          if (hResult < 0)
            throw Exception(
                "RunningObjectTable's GetObject() returned HRESULT: ${hResult.toRadixString(16)}");

          _vsInstance = dte2Pointer.value;

          final solutionNamePtr = calloc<Pointer<Utf16>>();
          final getSolutionName = (dte2Pointer.value.ref.lpVtbl.value + 6)
              .cast<
                  Pointer<
                      NativeFunction<
                          HRESULT Function(
                              Pointer, Pointer<Pointer<Utf16>>)>>>()
              .value
              .asFunction<int Function(Pointer, Pointer<Pointer<Utf16>>)>();

          getSolutionName(_vsInstance, solutionNamePtr);

          final solutionName = solutionNamePtr.value.toDartString();
          calloc.free(solutionNamePtr);

          if (solutionName == solutionPath) {
            break;
          }
        }

        moniker.release();
      }*/

    if (_vsInstance == nullptr) {
      _vsInstance = calloc<COMObject>();
      final clsid = calloc<GUID>();
      final iid = calloc<GUID>();

      hResult = CLSIDFromProgID(TEXT(_progId), clsid);
      if (FAILED(hResult)) throw COMException(hResult);
      hResult =
          IIDFromString(TEXT('{00000000-0000-0000-C000-000000000046}'), iid);
      if (FAILED(hResult)) throw COMException(hResult);

      hResult = CoCreateInstance(
          clsid, nullptr, CLSCTX.CLSCTX_LOCAL_SERVER, iid, _vsInstance.cast());
      print(hResult.toRadixString(16));
      if (FAILED(hResult)) throw COMException(hResult);

      /////////////////////////////////////////////////////////////////////////
      //}
      final dte = IDispatch(_vsInstance.cast());
      final riid = calloc<GUID>();
      final nameArray = calloc<Pointer<Utf16>>(1);
      nameArray[0] = TEXT('MainWindow');
      final dispId = calloc<Int32>();

      hResult = dte.getIDsOfNames(riid, nameArray, 1, 0, dispId);
      if (FAILED(hResult)) throw COMException(hResult);

      final pDispParams = calloc<DISPPARAMS>();
      final pVarResult = calloc<VARIANT>();

      hResult = dte.invoke(
          dispId.value,
          riid,
          0,
          DISPATCH_FLAGS.DISPATCH_PROPERTYGET,
          pDispParams,
          pVarResult,
          nullptr,
          nullptr);
      if (FAILED(hResult)) throw COMException(hResult);

      final mainWindowDispatch =
          IDispatch(pVarResult.ref.pdispVal as Pointer<COMObject>);

      nameArray[0] = TEXT('Activate');
      hResult = mainWindowDispatch.getIDsOfNames(riid, nameArray, 1, 0, dispId);
      if (FAILED(hResult)) throw COMException(hResult);

      hResult = mainWindowDispatch.invoke(
          dispId.value,
          riid,
          0,
          DISPATCH_FLAGS.DISPATCH_METHOD,
          pDispParams,
          nullptr,
          nullptr,
          nullptr);
      if (FAILED(hResult)) throw COMException(hResult);
//}
    }
  }
}
