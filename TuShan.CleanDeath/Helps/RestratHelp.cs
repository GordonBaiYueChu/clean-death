using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TuShan.BountyHunterDream.Setting.Setting;
using TuShan.BountyHunterDream.Setting;
using TuShan.BountyHunterDream.Logger;

namespace TuShan.CleanDeath.Helps
{
    public class RestratHelp
    {
        /// <summary>
        /// 启动设置录制恢复程序
        /// </summary>
        /// <param name="isRecordingRecovery"></param>
        public static bool RunRestartTools(bool isRecordingRecovery)
        {
            try
            {
                Process myprocess = new Process();
                string path = $"{AppDomain.CurrentDomain.BaseDirectory}ReocrdRestartTools.exe";
                string HEEGPath = $"{AppDomain.CurrentDomain.BaseDirectory}TuShan.CleanDeath.exe";
                string autoArg = "AutoStart";
                string prpoName = "TuShanCleanDeath";

                //参数 0.true 设为开机启动项 1.注册表 名称 2. 注册表 值 3.自启动参数 AutoStart
                //用; 分割
                string args = isRecordingRecovery.ToString() + ";" + prpoName + ";" + HEEGPath.ToString() + ";" + autoArg;

                ProcessStartInfo startInfo = new ProcessStartInfo(path, args);
                myprocess.StartInfo = startInfo;
                myprocess.Start();
                myprocess.WaitForExit();

                //退出码 1为正常，0为异常
                if (myprocess.ExitCode != 1)
                {
                    return false;
                }
                return true;
            }


            catch (Exception ex)
            {
                TLog.Info("Tools:Recording Recovery setting failed, Exception:" + ex.ToString());
                return false;
            }
        }
    }
}
