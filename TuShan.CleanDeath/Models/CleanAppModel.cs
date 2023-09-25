using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TuShan.CleanDeath.Models
{
     public class CleanAppModel : Caliburn.Micro.Screen
    {
        public CleanAppModel(string path)
        {
            AppExePath = path;
            IsEnable = true;
            GuidText = Guid.NewGuid().ToString();
        }

        public CleanAppModel()
        {
            IsEnable = true;
            GuidText = Guid.NewGuid().ToString();
        }

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
        /// 应用程序员exe路径，先关进程再卸载 C:\Program Files\Google\Chrome\Application\chrome.exe,0
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

        private bool _isEnable;

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnable
        {
            get { return _isEnable; }
            set
            {
                _isEnable = value;
                NotifyOfPropertyChange(() => IsEnable);
            }
        }

        public string GuidText { get; set; }

    }
}
