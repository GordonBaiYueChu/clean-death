using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TuShan.CleanDeath.Service.Utility
{
    public class WinAPI_Interop
    {
        public static IntPtr WTS_CURRENT_SERVER_HANDLE = IntPtr.Zero;
        public static void ShowServiceMessage(string message, string title)
        {
            int resp = 0;
            WTSSendMessage(WTS_CURRENT_SERVER_HANDLE, WTSGetActiveConsoleSessionId(), title, title.Length, message, message.Length, 0, 0, out resp, false);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int WTSGetActiveConsoleSessionId();

        [DllImport("wtsapi32.dll", SetLastError = true)]
        public static extern bool WTSSendMessage(IntPtr hServer, int SessionId, String pTitle, int TitleLength, String pMessage, int MessageLength, int Style, int Timeout, out int pResponse, bool bWait);
        #region P/Invoke WTS APIs
        private enum WTS_CONNECTSTATE_CLASS
        {
            WTSActive,
            WTSConnected,
            WTSConnectQuery,
            WTSShadow,
            WTSDisconnected,
            WTSIdle,
            WTSListen,
            WTSReset,
            WTSDown,
            WTSInit
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct WTS_SESSION_INFO
        {
            public UInt32 SessionID;
            public string pWinStationName;
            public WTS_CONNECTSTATE_CLASS State;
        }

        [DllImport("WTSAPI32.DLL", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool WTSEnumerateSessions(
            IntPtr hServer,
            [MarshalAs(UnmanagedType.U4)] UInt32 Reserved,
            [MarshalAs(UnmanagedType.U4)] UInt32 Version,
            ref IntPtr ppSessionInfo,
            [MarshalAs(UnmanagedType.U4)] ref UInt32 pSessionInfoCount
            );

        [DllImport("WTSAPI32.DLL", SetLastError = true, CharSet = CharSet.Auto)]
        static extern void WTSFreeMemory(IntPtr pMemory);

        [DllImport("WTSAPI32.DLL", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool WTSQueryUserToken(UInt32 sessionId, out IntPtr Token);
        #endregion

        #region P/Invoke CreateProcessAsUser
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct STARTUPINFO
        {
            public Int32 cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public Int32 dwX;
            public Int32 dwY;
            public Int32 dwXSize;
            public Int32 dwYSize;
            public Int32 dwXCountChars;
            public Int32 dwYCountChars;
            public Int32 dwFillAttribute;
            public Int32 dwFlags;
            public Int16 wShowWindow;
            public Int16 cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        [DllImport("ADVAPI32.DLL", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool CreateProcessAsUser(IntPtr hToken, string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes,
                                                      bool bInheritHandles, uint dwCreationFlags, string lpEnvironment, string lpCurrentDirectory,
                                                      ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("KERNEL32.DLL", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool CloseHandle(IntPtr hHandle);
        #endregion

        public static int CreateProcess(string ChildProcName, string workPath, string arguments)
        {
            IntPtr ppSessionInfo = IntPtr.Zero;
            UInt32 SessionCount = 0;
            int processID = 0;
            if (WTSEnumerateSessions(
                                    (IntPtr)WTS_CURRENT_SERVER_HANDLE,            
                                    0,         
                                    1,            
                                    ref ppSessionInfo,           
                                    ref SessionCount           
                                    ))
            {
                for (int nCount = 0; nCount < SessionCount; nCount++)
                {
                    WTS_SESSION_INFO tSessionInfo = (WTS_SESSION_INFO)Marshal.PtrToStructure(ppSessionInfo + nCount * Marshal.SizeOf(typeof(WTS_SESSION_INFO)), typeof(WTS_SESSION_INFO));
                    if (WTS_CONNECTSTATE_CLASS.WTSActive == tSessionInfo.State)
                    {
                        IntPtr hToken = IntPtr.Zero;
                        if (WTSQueryUserToken(tSessionInfo.SessionID, out hToken))
                        {
                            PROCESS_INFORMATION tProcessInfo;
                            STARTUPINFO tStartUpInfo = new STARTUPINFO();
                            tStartUpInfo.cb = Marshal.SizeOf(typeof(STARTUPINFO));
                            bool ChildProcStarted = CreateProcessAsUser(
                                                                        hToken,                   
                                                                        ChildProcName,              
                                                                        arguments,                       
                                                                        IntPtr.Zero,            
                                                                        IntPtr.Zero,            
                                                                        false,                    
                                                                        0,                        
                                                                        null,                   
                                                                        workPath,                   
                                                                        ref tStartUpInfo,        
                                                                        out tProcessInfo          
                                                     );
                            if (ChildProcStarted)
                            {
                                CloseHandle(tProcessInfo.hThread);
                                CloseHandle(tProcessInfo.hProcess);
                            }
                            else
                            {
                                ShowServiceMessage("CreateProcessAsUser失败", "CreateProcess");
                            }
                            CloseHandle(hToken);
                            processID = tProcessInfo.dwProcessId;
                            break;
                        }
                    }
                }
                WTSFreeMemory(ppSessionInfo);
            }
            return processID;
        }
    }
}
