using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using TuShan.BountyHunterDream.Logger;
using TuShan.CleanDeath.Helps;
using TuShan.CleanDeath.Service.Struct;

namespace TuShan.CleanDeath
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            if (IsAutoStartModel(e.Args))
            {
                ServiceUtility.StartService();
                Environment.Exit((int)ProcessStateCodeEnum.ExitCodeSuccess);
                return;
            }
            //配置日志文件信息
            TLog.Configure("../Conf/Factory/log4net.config");
            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            //设置工作目录
            System.IO.Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            Task.Run(() =>
            {
                //阻止自动休眠
                OperateWindows.PreventAutoSleep();
            });
            //关闭可能存在的服务
            ServiceUtility.StopService();
            base.OnStartup(e);
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
