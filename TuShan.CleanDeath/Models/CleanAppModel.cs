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
