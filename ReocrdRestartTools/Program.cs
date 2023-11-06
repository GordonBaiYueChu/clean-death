using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ReocrdRestartTools
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string argsData = string.Empty;
                foreach (string temp in args)
                {
                    argsData += temp + " ";
                }
                argsData = argsData.TrimEnd();

                if (!argsData.Contains(";"))
                {
                    return;
                }
                string[] needArgs = argsData.Split(';');
                if (needArgs.Count() < 4)
                {
                    return;
                }

                bool isContinueRecord = Convert.ToBoolean(needArgs[0]);
                string prpoName = needArgs[1];
                string prpoPath = needArgs[2];
                string autoArg = needArgs[3];
                RegistryKey registryRoot = Registry.CurrentUser;

                string runRootPath = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

                RegistryKey runRoot = registryRoot.OpenSubKey(runRootPath, true);

                ChangedRegistryValue(isContinueRecord, runRoot, prpoName, prpoPath + " " + autoArg);

                Environment.Exit(1);
            }
            catch (Exception ex)
            {
                Environment.Exit(0);
            }

        }

        private static void ChangedRegistryValue(bool isContinueRecord, RegistryKey runRoot, string prpoName, string value)
        {
            if (runRoot == null)
            {
                return;
            }

            if (isContinueRecord)
            {
                if (RegistryHelp.IsRegeditKeyExist(runRoot, prpoName))
                {
                    if (RegistryHelp.ReadPropValue(runRoot, prpoName) != value)
                    {
                        RegistryHelp.Update(runRoot, prpoName, value, RegistryValueKind.String);
                    }
                }
                else
                {
                    RegistryHelp.Update(runRoot, prpoName, value, RegistryValueKind.String);
                }
            }
            else
            {
                if (RegistryHelp.IsRegeditKeyExist(runRoot, prpoName))
                {
                    RegistryHelp.Update(runRoot, prpoName, "", RegistryValueKind.String);
                }
            }

            runRoot.Close();
        }
    }
}
