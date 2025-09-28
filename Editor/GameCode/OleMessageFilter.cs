using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Editor.GameCode
{
    internal class OleMessageFilter : IOleMessageFilter
    {
        private const int SERVERCALL_ISHANDLED = 0;
        private const int PENDINGMSG_WAITDEFPROCESS = 2;
        private const int SERVERCALL_RETRYLATER = 2;

        [DllImport("Ole32.dll")]
        private static extern int CoRegisterMessageFilter(IOleMessageFilter newFilter, out IOleMessageFilter oldFilter);

        public static void Register()
        {
            IOleMessageFilter newFilter = new OleMessageFilter();
            int hr = CoRegisterMessageFilter(newFilter, out var oldFilter);

            Debug.Assert(hr >= 0, "Registering COM IMessageFilter failed.");
        }

        public static void Revoke()
        {
            int hr = CoRegisterMessageFilter(null!, out var oldFilter);

            Debug.Assert(hr >= 0, "Unregistering COM IMessageFilter failed");
        }

        public int HandleInComingCall(int dwCallType, IntPtr hTaskCaller, int dwTickCount, IntPtr lpInterfaceInfo)
            => SERVERCALL_ISHANDLED;

        public int RetryRejectedCall(IntPtr hTaskCallee, int dwTickCount, int dwRejectType)
        {
            if (dwRejectType == SERVERCALL_RETRYLATER)
            {
                Debug.WriteLine("COM server busy. Retrying call to EnvDTE interface.");

                return 500;
            }

            return -1;
        }

        public int MessagePending(IntPtr hTaskCallee, int dwTickCount, int dwPendingType) => PENDINGMSG_WAITDEFPROCESS;
    }
}
