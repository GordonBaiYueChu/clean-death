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
        public static bool RunRestartTools(bool isRecordingRecovery)
        {
            try
            {
                Process myprocess = new Process();
                string path = $"{AppDomain.CurrentDomain.BaseDirectory}ReocrdRestartTools.exe";
                string autoStartProcessPath = $"{AppDomain.CurrentDomain.BaseDirectory}TuShan.CleanDeath.exe";
                string autoArg = "AutoStart";
                string prpoName = "TuShanCleanDeath";

                string args = isRecordingRecovery.ToString() + ";" + prpoName + ";" + autoStartProcessPath.ToString() + ";" + autoArg;

                ProcessStartInfo startInfo = new ProcessStartInfo(path, args);
                myprocess.StartInfo = startInfo;
                myprocess.Start();
                myprocess.WaitForExit();

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
