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
            TLog.Configure("../Conf/Factory/log4net.config");
            this.Visible = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CleanDeathSetting cleanDeathSetting = SettingUtility.GetTSetting<CleanDeathSetting>();
            DeleteLnkOnTask(cleanDeathSetting.CleanApps);
            this.Close();
        }


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
                        if (CleanApps.Any(c => c.IsEnable &&( fileee.Contains(c.AppExeName) || c.AppExeName.Contains(lnkName))))
                        {
                            Shell shell = new Shell();
                            Folder folder = shell.NameSpace(Path.GetDirectoryName(file));
                            FolderItem app = folder.ParseName(Path.GetFileName(file));
                            foreach (FolderItemVerb Fib in app.Verbs())
                            {
                                if (Fib.Name.Contains("从任务"))
                                {
                                    Fib.DoIt();
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    TLog.Error($"删除任务栏快捷方式时出现错误：{ex},lnk名称{file}");
                }
            }
        }

        private string GetPathLinkName(string path)
        {
            string[] strings = path.Split('\\');
            string name = strings[strings.Length - 1].Replace(".lnk", "");
            return name;
        }

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
                Console.WriteLine("发生异常：" + ex.Message);
                return null;
            }
        }

    }
}
