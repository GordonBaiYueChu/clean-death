using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using TuShan.BountyHunterDream.Logger;
using TuShan.BountyHunterDream.Setting;
using TuShan.CleanDeath.Helps;
using TuShan.CleanDeath.Service.Struct;
using TuShan.CleanDeath.Views;

namespace TuShan.CleanDeath
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            TLog.Configure("../Conf/Factory/log4net.config");
            if (IsAutoStartModel(e.Args))
            {
                TLog.Info("自启动");
                ServiceUtility.StartService();
                Environment.Exit((int)ProcessStateCodeEnum.ExitCodeSuccess);
                return;
            }
            var processes = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
            if (processes.Length > 1)
            {
                if (MyMessageBox.Show("已有相同应用再运行，是否重新打开？", ButtonType.YesNo,
               MessageType.Question) == MessageBoxResult.Yes)
                {
                    ServiceUtility.StopService();
                    KillExistsProcess(processes);
                }
                else
                {
                    TLog.Info("Exit because of exist one main role.");
                    return;
                }
            }
            SettingUtility.CheckFilesExist();
            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            System.IO.Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            Task.Run(() =>
            {
                OperateWindows.PreventAutoSleep();
            });
            base.OnStartup(e);
        }

        private void KillExistsProcess(Process[] processes)
        {
            int currentProcessId = Process.GetCurrentProcess().Id;
            foreach (Process process in processes)
            {
                if (process.Id != currentProcessId)
                {
                    try
                    {
                        process.Kill();
                    }
                    catch (Exception)
                    {
                        process.Kill();
                    }
                }
            }
        }

        private const string AUTO_START = "AutoStart";

        private bool IsAutoStartModel(string[] args)
        {
            if (args != null && args.Length == 1 && args[0].Equals(AUTO_START))
            {
                return true;
            }
            return false;
        }

        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            if (e.Exception != null)
            {
                StringBuilder sb = new StringBuilder();
                RecordExceptionToSB(ref sb, e.Exception);
                TLog.Error(sb.ToString());
            }
        }

        private void RecordExceptionToSB(ref StringBuilder sb, Exception ex)
        {
            if (ex != null)
            {
                sb.Append("Process ID" + Process.GetCurrentProcess().Id.ToString() + "    ");
                sb.Append($"Exception:{ex.Message} StackTrace:");
                sb.AppendLine();
                sb.Append($"{ex.StackTrace}");
                var exInner = ex.InnerException;
                while (exInner != null)
                {
                    sb.AppendLine();
                    sb.Append($"Inner Exception:{exInner.Message} Inner StackTrace:");
                    sb.AppendLine();
                    sb.Append($"{exInner.StackTrace}");
                    exInner = exInner.InnerException;
                }
            }
        }


        protected override void OnExit(ExitEventArgs e)
        {
            OperateWindows.ResumeAutoSleep();
            base.OnExit(e);
        }
    }
}
