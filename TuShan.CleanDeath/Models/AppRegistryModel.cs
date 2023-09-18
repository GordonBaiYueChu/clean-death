using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TuShan.CleanDeath.Models
{
    public class AppRegistryModel : Caliburn.Micro.Screen
    {
        private string appDisplayName;

        /// <summary>
        /// 应用程序名称
        /// </summary>
        public string AppDisplayName
        {
            get { return appDisplayName; }
            set
            {
                appDisplayName = value;
                NotifyOfPropertyChange(() => AppDisplayName);
            }
        }

        private string installLocation;

        /// <summary>
        /// 安装目录 例如：C:\Program Files\Google\Chrome\Application
        /// </summary>
        public string InstallLocation
        {
            get { return installLocation; }
            set
            {
                installLocation = value;
                NotifyOfPropertyChange(() => InstallLocation);
            }
        }

        private string appExeName;

        /// <summary>
        /// 应用程序员exe名称chrome.exe
        /// </summary>
        public string AppExeName
        {
            get { return appExeName; }
            set
            {
                appExeName = value;
                NotifyOfPropertyChange(() => AppExeName);
            }
        }

        private string appExePath;

        /// <summary>
        /// 应用程序员exe名称，先关进程再卸载 C:\Program Files\Google\Chrome\Application\chrome.exe,0
        /// </summary>
        public string AppExePath
        {
            get { return appExePath; }
            set
            {
                appExePath = value;
                NotifyOfPropertyChange(() => AppExePath);
            }
        }


        private string unInstallString;

        /// <summary>
        /// 卸载命令
        /// </summary>
        public string UnInstallString
        {
            get { return unInstallString; }
            set
            {
                unInstallString = value;
                NotifyOfPropertyChange(() => UnInstallString);
            }
        }


    }
}
