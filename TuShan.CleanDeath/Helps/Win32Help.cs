using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TuShan.CleanDeath.Helps
{
    public class Win32Help
    {
        public void zass()
        {
            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;

            Console.WriteLine("等待系统事件...");
            Console.ReadLine();

        }

        private static void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (e.Reason == SessionSwitchReason.SessionLock)
            {
                Console.WriteLine("系统已锁屏.");
            }
            else if (e.Reason == SessionSwitchReason.SessionUnlock)
            {
                Console.WriteLine("系统已解锁.");
            }
        }

    }

    public static class OperateWindows
    {
        #region About Window
        [DllImport("User32.dll")]
        public static extern void SwitchToThisWindow(IntPtr hWnd, bool turnon);
        #endregion

        #region About OutPut
        [DllImport("kernel32.dll")]
        public static extern Boolean AllocConsole();
        [DllImport("kernel32.dll")]
        public static extern Boolean FreeConsole();
        #endregion

        #region About System Shutdown
        public enum ShutdownType
        {
            User = 1,
            Power
        }

        public static event Action<(ShutdownType type, string msg)> ShutdownRaisedEvent;

        static OperateWindows()
        {
            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
        }


        private static void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            var arg = (ShutdownType.User, $"SessionSwitch {e.Reason}");
            ShutdownRaisedEvent?.Invoke(arg);
        }

        private static void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Suspend)
            {
                var arg = (ShutdownType.Power, $"PowerModeChanged {e.Mode}");
                ShutdownRaisedEvent?.Invoke(arg);
            }
        }

        private static void SystemEvents_SessionEnding(object sender, Microsoft.Win32.SessionEndingEventArgs e)
        {
            e.Cancel = true;
            var arg = (ShutdownType.User, $"SessionEnding {e.Reason}");
            ShutdownRaisedEvent?.Invoke(arg);
        }
        #endregion

        #region About Sleep
        [DllImport("kernel32.dll")]
        static extern uint SetThreadExecutionState(SLEEP_EXECUTION_FLAG flags);

        [Flags]
        enum SLEEP_EXECUTION_FLAG : uint
        {
            SYSTEM = 0x00000001,
            DISPLAY = 0x00000002,
            CONTINUOUS = 0x80000000,
        }

        public static void PreventAutoSleep(bool includeDisplay = true)
        {
            var flag = SLEEP_EXECUTION_FLAG.CONTINUOUS | SLEEP_EXECUTION_FLAG.SYSTEM;
            if (includeDisplay)
                flag |= SLEEP_EXECUTION_FLAG.DISPLAY;
            SetThreadExecutionState(flag);
        }

        public static void ResumeAutoSleep()
        {
            SetThreadExecutionState(SLEEP_EXECUTION_FLAG.CONTINUOUS);
        }

        public static void ResetAutoSleepTime(bool includeDisplay = true)
        {
            var flag = SLEEP_EXECUTION_FLAG.SYSTEM;
            if (includeDisplay)
                flag |= SLEEP_EXECUTION_FLAG.DISPLAY;
            SetThreadExecutionState(flag);
        }
        #endregion

        #region About Screen Saveactive
        [DllImport("user32.dll")]
        static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref bool pvParam, uint fWinIni);
        [DllImport("user32.dll")]
        static extern bool SystemParametersInfo(uint uiAction, bool uiParam, uint pvParam, uint fWinIni);
        const uint SPI_GETSCREENSAVEACTIVE = 0x0010;
        const uint SPI_SETSCREENSAVEACTIVE = 0x0011;
        const uint SPIF_SENDCHANGE = 0x0002;
        const uint SPIF_SENDWININICHANGE = SPIF_SENDCHANGE;

        public static bool GetScreenSaveactive()
        {
            bool active = false;
            SystemParametersInfo(SPI_GETSCREENSAVEACTIVE, 0, ref active, 0);
            return active;
        }

        public static void CloseScreenSaveactive()
        {
            if (GetScreenSaveactive())
                SystemParametersInfo(SPI_SETSCREENSAVEACTIVE, false, 0, 0);
        }

        public static void OpenScreenSaveactive()
        {
            if (!GetScreenSaveactive())
                SystemParametersInfo(SPI_SETSCREENSAVEACTIVE, true, 0, 0);
        }
        #endregion
    }
}
