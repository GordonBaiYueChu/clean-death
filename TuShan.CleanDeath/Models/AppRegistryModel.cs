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


        private string unInstallString;

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
