using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TuShan.CleanDeath.Helps
{
    public class PathHelp
    {
        /// <summary>
        /// 获取lnk文件的目标文件地址
        /// </summary>
        /// <param name="shortcutPath"></param>
        /// <returns></returns>
        public static string GetShortcutTarget(string shortcutPath)
        {
            try
            {
                Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                object shell = Activator.CreateInstance(shellType);
                object shortcut = shellType.InvokeMember("CreateShortcut", BindingFlags.InvokeMethod, null, shell, new object[] { shortcutPath });
                string targetPath = (string)shortcut.GetType().InvokeMember("TargetPath", BindingFlags.GetProperty, null, shortcut, null);
                return targetPath;
            }
            catch (Exception ex)
            {
                // 处理异常，如快捷方式文件不存在或无法读取
                Console.WriteLine("发生异常：" + ex.Message);
                return null;
            }
        }
    }
}
