using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace TuShan.CleanDeath.Models
{
    public class CleanFolderModel : Caliburn.Micro.Screen
    {
        public CleanFolderModel(string path)
        {
            CleanFolderPath = path;
            IsEnable = true;
            GuidText = Guid.NewGuid().ToString();
        }

        public CleanFolderModel()
        {
            IsEnable = true;
            GuidText = Guid.NewGuid().ToString();
        }

        private string _cleanFolderPath;

        public string CleanFolderPath
        {
            get { return _cleanFolderPath; }
            set
            {
                _cleanFolderPath = value;
                NotifyOfPropertyChange(() => CleanFolderPath);
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

    public class OpenProcessStruct
    {
        public string Path { get; set; }
        public string Args { get; set; }
        public bool WithWindow { get; set; }
    }
}
