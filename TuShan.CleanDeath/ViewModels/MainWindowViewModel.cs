using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Documents;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using TuShan.BountyHunterDream.Logger;
using TuShan.BountyHunterDream.Setting;
using TuShan.BountyHunterDream.Setting.Setting;
using TuShan.BountyHunterDream.Setting.Struct;
using TuShan.CleanDeath.Helps;
using TuShan.CleanDeath.Models;

namespace TuShan.CleanDeath.ViewModels
{
    public class MainWindowViewModel : Caliburn.Micro.Screen
    {

        public MainWindowViewModel()
        {
            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
        }

        private ObservableCollection<CleanFolderModel> _cleanFolders = new ObservableCollection<CleanFolderModel>();

        /// <summary>
        /// 监控文件夹地址集合
        /// </summary>
        public ObservableCollection<CleanFolderModel> CleanFolders
        {
            get { return _cleanFolders; }
            set
            {
                _cleanFolders = value;
                NotifyOfPropertyChange(() => CleanFolders);
            }
        }


        private CleanFolderModel _selectedCleanFolderItem;

        /// <summary>
        /// 选中的监控文件夹
        /// </summary>
        public CleanFolderModel SelectedCleanFolderItem
        {
            get { return _selectedCleanFolderItem; }
            set
            {
                _selectedCleanFolderItem = value;
                NotifyOfPropertyChange(() => SelectedCleanFolderItem);
            }
        }

        private int _maxTimeOutDay;

        public int MaxTimeOutDay
        {
            get { return _maxTimeOutDay; }
            set
            {
                _maxTimeOutDay = value;
                NotifyOfPropertyChange(() => MaxTimeOutDay);
            }
        }


        /// <summary>
        /// 加载配置数据
        /// </summary>
        public void CleanFoldersLoaded()
        {
            string path = "D:\\cleandeath\\cleandeathold";
            CleanFolderModel cleanFolderModel = new CleanFolderModel(path);
            if (CleanFolders != null)
            {
                CleanFolders = new ObservableCollection<CleanFolderModel>();
            }
            CleanFolders.Add(cleanFolderModel);
            if (CleanFolders != null && CleanFolders.Count > 0)
            {
                SelectedCleanFolderItem = CleanFolders[0];
            }
        }

        /// <summary>
        /// 选择要监控的文件夹
        /// </summary>
        public void SelectedFolder()
        {
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog
            {
                Description = "选择文件夹",
                UseDescriptionForTitle = true // 使用描述作为对话框的标题
            };
            if (dialog.ShowDialog() == true)
            {
                if (SelectedCleanFolderItem != null)
                {
                    SelectedCleanFolderItem.CleanFolderPath = dialog.SelectedPath;
                }
            }
        }

        /// <summary>
        /// 更新启用的值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void UseOnActiveChecked(object sender, System.Windows.RoutedEventArgs e)
        {
            System.Windows.Controls.CheckBox box = sender as System.Windows.Controls.CheckBox;

            CleanFolderModel model = (CleanFolderModel)box.DataContext;
            foreach (CleanFolderModel item in CleanFolders)
            {

                if (item.GuidText == model.GuidText)
                {
                    item.IsEnable = (bool)box.IsChecked;
                }
            }
        }

        /// <summary>
        /// 删除监护文件夹
        /// </summary>
        public void DeleteSelectedFolder()
        {
            if (CleanFolders == null)
            {
                return;
            }
            CleanFolders.Remove(SelectedCleanFolderItem);
            if (CleanFolders.Count > 0)
            {
                SelectedCleanFolderItem = CleanFolders[0];
            }
        }

        /// <summary>
        /// 添加监护文件夹
        /// </summary>
        public void AddCleanFolderEvent()
        {
            CleanFolderModel cleanFolderModel = new CleanFolderModel();
            if (CleanFolders == null)
            {
                CleanFolders = new ObservableCollection<CleanFolderModel>();
            }
            CleanFolders.Add(cleanFolderModel);
        }

        /// <summary>
        /// 保存用户选中的数据
        /// </summary>
        public void SaveCleanFolderEvent()
        {
            CleanDeathSetting cleanDeathSetting = SettingUtility.GetTSetting<CleanDeathSetting>();
            if (cleanDeathSetting == null)
            {
                cleanDeathSetting = new CleanDeathSetting();
            }
            List<CleanFolderModel> newList = new List<CleanFolderModel>();
            foreach (CleanFolderModel cleanFolderModel in CleanFolders)
            {
                if (string.IsNullOrWhiteSpace(cleanFolderModel.CleanFolderPath))
                {
                    continue;
                }
                newList.Add(cleanFolderModel);
                StructCleanFolder structCleanFolder = new StructCleanFolder();
                structCleanFolder.FolderPath = cleanFolderModel.CleanFolderPath;
                structCleanFolder.IsEnable = cleanFolderModel.IsEnable;
                cleanDeathSetting.CleanFolders.Add(structCleanFolder);
            }
            CleanFolders = new ObservableCollection<CleanFolderModel>(newList);
        }

        public void StartGuard()
        {
            //启动服务 服务感知不到windows的锁屏，现在用的内存也不大，暂时用应用程序
            //ServiceUtility.StartService();
            //this.TryCloseAsync();
        }

        public void ViewModelClosed()
        {
            SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;
        }

        public void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            switch (e.Reason)
            {
                case SessionSwitchReason.SessionLock:
                    break;
                case SessionSwitchReason.SessionUnlock:
                    break;
                case SessionSwitchReason.SessionLogon:
                    break;
                default:
                    break;
            }
        }
    }
}
