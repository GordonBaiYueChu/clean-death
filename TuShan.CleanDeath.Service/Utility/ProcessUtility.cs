using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TuShan.BountyHunterDream.Logger;
using TuShan.CleanDeath.Service.Struct;

namespace TuShan.CleanDeath.Service.Utility
{
    public class ProcessUtility
    {
        public static int OpenMainProcess(OpenProcessStruct model = null)
        {
            string exePath = $"{AppDomain.CurrentDomain.BaseDirectory}TuShan.BountyHunterDream.Viewer.BHD.exe";
            if (model == null)
            {
                model = new OpenProcessStruct
                {
                    Args = "",
                    Path = exePath,
                    WithWindow = false
                };
            }
            return WinAPI_Interop.CreateProcess(model.Path, Path.GetDirectoryName(model.Path), model.Args);
        }

        public static void KillProcess(int processID)
        {

            try
            {
                var processes = Process.GetProcessById(processID);
                if (!processes.HasExited)
                {
                    TLog.Info($"Kill {processes.ProcessName}");
                    try
                    {
                        if (!processes.CloseMainWindow())
                        {
                            processes.Kill();
                        }
                    }
                    catch (Exception)
                    {
                        processes.Kill();
                    }
                    processes.WaitForExit(1000);
                    processes.Dispose();
                }
            }
            catch (Exception ex)
            {

            }
        }


    }
}
