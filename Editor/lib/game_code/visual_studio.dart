import 'dart:ffi';
import 'package:ffi/ffi.dart';
import 'package:win32/win32.dart';

class VisualStudio {

  static Pointer<COMObject>? _vsInstance;
  static final String _progId = "VisualStudio.DTE.17.0";

  static void openVisualStudio(String solutionPath) {
    if(_vsInstance == null) {
      if(_vsInstance == null) {
        var clsid = calloc<GUID>();
        var progIdPtr = _progId.toNativeUtf16();
        var typeFromProgId = CLSIDFromProgID(progIdPtr, clsid);
        calloc.free(progIdPtr);

        if(typeFromProgId < 0) {
          throw COMException(-1, message: 'CLSIDFromProgId() failed');
        }

        _vsInstance = calloc<COMObject>();

        var hr = CoCreateInstance(clsid, nullptr, CLSCTX.CLSCTX_ALL, clsid, _vsInstance!.cast());

        if(FAILED(hr)) {
          throw COMException(-1, message: 'CoCreateInstance() failed: $hr');
        }
      }
    }
  }

  VisualStudio._();
}