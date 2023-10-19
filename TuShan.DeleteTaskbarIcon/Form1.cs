using Shell32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TuShan.BountyHunterDream.Setting.Setting;
using TuShan.BountyHunterDream.Setting;
using TuShan.BountyHunterDream.Setting.Struct;
using System.Reflection;
using TuShan.BountyHunterDream.Logger;

namespace TuShan.DeleteTaskbarIcon
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            //配置日志文件信息
            TLog.Configure("../Conf/Factory/log4net.config");
            this.Visible = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CleanDeathSetting cleanDeathSetting = SettingUtility.GetTSetting<CleanDeathSetting>();
            //删除任务栏快捷方式
            DeleteLnkOnTask(cleanDeathSetting.CleanApps);
            TLog.Info("关闭");
            this.Close();
        }


        /// <summary>
        /// 删除任务栏上的快捷方式
        /// </summary>
        /// <param name="CleanApps"></param>
        private void DeleteLnkOnTask(List<AppSetttingStruct> CleanApps)
        {
            string localFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string parentDirectory = Directory.GetParent(localFolderPath).FullName;
            string roamingFolderPath = Path.Combine(parentDirectory, "Roaming");
            string path = Path.Combine(roamingFolderPath, "Microsoft", "Internet Explorer", "Quick Launch", "User Pinned", "TaskBar");
            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                try
                {
                    if (file.Contains(".lnk"))
                    {
                        string fileee = GetShortcutTarget(file);
                        string lnkName = GetPathLinkName(file);
                        if (CleanApps.Any(c => fileee.Contains(c.AppExeName) || c.AppExeName.Contains(lnkName)))
                        {
                            Shell shell = new Shell();
                            Folder folder = shell.NameSpace(Path.GetDirectoryName(file));
                            FolderItem app = folder.ParseName(Path.GetFileName(file));
                            foreach (FolderItemVerb Fib in app.Verbs())
                            {
                                //从任务栏取消固定(&K)
                                if (Fib.Name.Contains("从任务"))
                                {
                                    Fib.DoIt();
                                }
                            }
                           // File.Delete(file);
                        }
                    }
                }
                catch (Exception ex)
                {
                    TLog.Error($"删除任务栏快捷方式时出现错误：{ex},lnk名称{file}");
                }
            }
        }

        /// <summary>
        /// 获取lnk的名称
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string GetPathLinkName(string path)
        {
            string[] strings = path.Split('\\');
            string name = strings[strings.Length - 1].Replace(".lnk", "");
            return name;
        }

        /// <summary>
        /// 获取快捷方式的目标地址
        /// </summary>
        /// <param name="shortcutPath"></param>
        /// <returns></returns>
        string GetShortcutTarget(string shortcutPath)
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
