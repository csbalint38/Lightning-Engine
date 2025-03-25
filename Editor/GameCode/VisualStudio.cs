using Editor.Common.Enums;
using Editor.Utilities;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Editor.GameCode
{
    static class VisualStudio
    {
        private static readonly string _progId = "VisualStudio.DTE.17.0";
        private static EnvDTE80.DTE2 _vsInstance = null;

        [DllImport("ole32.dll")]
        private static extern int GetRunningObjectTable(uint reserved, out IRunningObjectTable rot);

        [DllImport("ole32.dll")]
        private static extern int CreateBindCtx(uint reserved, out IBindCtx ppbc);

        public static void OpenVisualStudio(string solutionPath)
        {
            IRunningObjectTable rot = null;
            IEnumMoniker monikerTable = null;
            IBindCtx bindCtx = null;

            try
            {
                if (_vsInstance is null)
                {
                    var hResult = GetRunningObjectTable(0, out rot);

                    if (hResult < 0 || rot is null)
                    {
                        throw new COMException($"GetRunningObjecctTable() returned HRESULT {hResult:X8}");
                    }

                    rot.EnumRunning(out monikerTable);
                    monikerTable.Reset();

                    hResult = CreateBindCtx(0, out bindCtx);

                    if (hResult < 0 || bindCtx is null)
                    {
                        throw new COMException($"CreateBindCtx() returned HRESULT {hResult:X8}");
                    }

                    IMoniker[] currentMoniker = new IMoniker[1];

                    while(monikerTable.Next(1, currentMoniker, IntPtr.Zero) == 0)
                    {
                        string displayName = string.Empty;

                        currentMoniker[0]?.GetDisplayName(bindCtx, null, out displayName);

                        if(displayName.Contains(_progId))
                        {
                            hResult = rot.GetObject(currentMoniker[0], out object obj);

                            if(hResult < 0 || obj is null)
                            {
                                throw new COMException($"GetObject() returned HRESULT {hResult:X8}");
                            }

                            EnvDTE80.DTE2 dte = obj as EnvDTE80.DTE2;
                            var solutionName = dte.Solution.FullName;

                            if (solutionName == solutionPath)
                            {
                                _vsInstance = dte;
                                break;
                            }
                        }
                    } 

                    if (_vsInstance is null)
                    {
                        Type vsType = Type.GetTypeFromProgID(_progId, true);
                        _vsInstance = Activator.CreateInstance(vsType) as EnvDTE80.DTE2;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Logger.Log(LogLevel.ERROR, ex.Message);
            }
            finally
            {
                if(monikerTable is not null) Marshal.ReleaseComObject(monikerTable);
                if (rot is not null) Marshal.ReleaseComObject(rot);
                if (bindCtx is not null) Marshal.ReleaseComObject(bindCtx);
            }
        }

        public static void CloseVisualStudio()
        {
            if (_vsInstance?.Solution.IsOpen == true)
            {
                _vsInstance.ExecuteCommand("File.SaveAll");
                _vsInstance.Solution.Close(true);
            }
        }
    }
}
