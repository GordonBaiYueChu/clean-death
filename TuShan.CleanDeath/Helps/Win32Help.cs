using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;

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
                // 在锁屏时执行操作
            }
            else if (e.Reason == SessionSwitchReason.SessionUnlock)
            {
                Console.WriteLine("系统已解锁.");
                // 在解锁时执行操作
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
            //这个阻止关机没有工作
            //SystemEvents.SessionEnding += SystemEvents_SessionEnding;
            // SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
        }


        /// <summary>
        /// 注销，登录 停止录制。锁屏时也会停止录制，开机自启动在解锁后停止录制。不合理。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /*
         * https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-setthreadexecutionstate
         */
        [DllImport("kernel32.dll")]
        static extern uint SetThreadExecutionState(SLEEP_EXECUTION_FLAG flags);

        /// <summary>
        /// 只使用Continuous参数时，则是恢复系统休眠策略。
        /// 不使用Continuous参数时，实现阻止系统休眠或显示器关闭一次
        /// 组合使用Continuous参数时，实现阻止系统休眠或显示器关闭至线程终止
        /// </summary>
        [Flags]
        enum SLEEP_EXECUTION_FLAG : uint
        {
            SYSTEM = 0x00000001,
            DISPLAY = 0x00000002,
            CONTINUOUS = 0x80000000,
        }

        /// <summary>
        /// 阻止系统休眠，直到调用ResumeAutoSleep恢复
        /// </summary>
        /// <param name="includeDisplay"></param>
        public static void PreventAutoSleep(bool includeDisplay = true)
        {
            var flag = SLEEP_EXECUTION_FLAG.CONTINUOUS | SLEEP_EXECUTION_FLAG.SYSTEM;
            if (includeDisplay)
                flag |= SLEEP_EXECUTION_FLAG.DISPLAY;
            SetThreadExecutionState(flag);
        }

        /// <summary>
        /// 恢复原自动休眠策略
        /// </summary>
        public static void ResumeAutoSleep()
        {
            SetThreadExecutionState(SLEEP_EXECUTION_FLAG.CONTINUOUS);
        }

        /// <summary>
        /// 重置自动休眠计时器一次
        /// </summary>
        /// <param name="includeDisplay"></param>
        public static void ResetAutoSleepTime(bool includeDisplay = true)
        {
            var flag = SLEEP_EXECUTION_FLAG.SYSTEM;
            if (includeDisplay)
                flag |= SLEEP_EXECUTION_FLAG.DISPLAY;
            SetThreadExecutionState(flag);
        }
        #endregion

        #region About Screen Saveactive
        /*
         * https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-systemparametersinfoa
         * https://support.microsoft.com/zh-cn/help/318781/bug-systemparametersinfo-with-spi-getscreensaveactive-always-returns-t
         * 错误︰ SystemParametersInfo 与 SPI_GETSCREENSAVEACTIVE 总是返回 True 在 Windows 2000 上[windows bug，上述连接地址有解决方案]
         */
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
