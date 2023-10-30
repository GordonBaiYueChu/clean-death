using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using TuShan.BountyHunterDream.Logger;
using TuShan.CleanDeath.Helps;
using TuShan.CleanDeath.Service.Struct;
using TuShan.CleanDeath.Views;

namespace TuShan.CleanDeath
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            //配置日志文件信息
            TLog.Configure("../Conf/Factory/log4net.config");
            if (IsAutoStartModel(e.Args))
            {
                TLog.Info("自启动");
                ServiceUtility.StartService();
                Environment.Exit((int)ProcessStateCodeEnum.ExitCodeSuccess);
                return;
            }
            //防止重复启动
            //检查Main进程重复打开
            var processes = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
            if (processes.Length > 1)
            {
                //为了让用户在遇到未知的问题时，可以通过重启应用来解决。保证软件在任何时候都可以启动。
                if (MyMessageBox.Show("已有相同应用再运行，是否重新打开？", ButtonType.YesNo,
               MessageType.Question) == MessageBoxResult.Yes)
                {
                    ServiceUtility.StopService();
                    //关闭以前的存在的程序，本程序继续执行即可。
                    KillExistsProcess(processes);
                }
                else
                {
                    TLog.Info("Exit because of exist one main role.");
                    return;
                }
            }
            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            //设置工作目录
            System.IO.Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            Task.Run(() =>
            {
                //阻止自动休眠
                OperateWindows.PreventAutoSleep();
            });
            //关闭可能存在的服务
            //ServiceUtility.StopService();
            base.OnStartup(e);
        }

        /// <summary>
        /// 关闭已运行的应用
        /// </summary>
        /// <param name="processes"></param>
        private void KillExistsProcess(Process[] processes)
        {
            int currentProcessId = Process.GetCurrentProcess().Id;
            foreach (Process process in processes)
            {
                if (process.Id != currentProcessId)
                {
                    try
                    {
                        //录制中，关闭程序会触发异常恢复逻辑，所以直接kill（已停止服务不会恢复）
                        process.Kill();
                    }
                    catch (Exception)
                    {
                        process.Kill();
                    }
                }
            }
        }

        /// <summary>
        /// 开机自启的指定启动参数
        /// </summary>
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
            //设置为已经处理
            e.Handled = true;
            //记录
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
            //恢复系统休眠机制
            OperateWindows.ResumeAutoSleep();
            base.OnExit(e);
        }
    }
}
