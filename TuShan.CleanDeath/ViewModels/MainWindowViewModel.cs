using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Xml.Linq;
using Caliburn.Micro;
using HandyControl.Controls;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using TuShan.BountyHunterDream.Logger;
using TuShan.BountyHunterDream.Setting;
using TuShan.BountyHunterDream.Setting.Setting;
using TuShan.BountyHunterDream.Setting.Struct;
using TuShan.CleanDeath.Helps;
using TuShan.CleanDeath.Models;
using TabControl = System.Windows.Controls.TabControl;

namespace TuShan.CleanDeath.ViewModels
{
    public class MainWindowViewModel : Caliburn.Micro.Screen
    {
        private readonly IWindowManager _iWindowManager;

        public MainWindowViewModel()
        {
            _iWindowManager = IoC.Get<IWindowManager>();
        }

        #region 文件夹设置相关

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
            CleanDeathSetting cleanDeathSetting = SettingUtility.GetTSetting<CleanDeathSetting>();
            if (cleanDeathSetting == null)
            {
                cleanDeathSetting = new CleanDeathSetting();
            }

            List<CleanFolderModel> cleanFolderModels = new List<CleanFolderModel>();
            foreach (StructCleanFolder structCleanFolder in cleanDeathSetting.CleanFolders)
            {
                CleanFolderModel cleanFolderModel = new CleanFolderModel();
                cleanFolderModel.CleanFolderPath = structCleanFolder.FolderPath;
                cleanFolderModel.IsEnable = structCleanFolder.IsEnable;
                cleanFolderModels.Add(cleanFolderModel);
            }

            CleanFolders = new ObservableCollection<CleanFolderModel>(cleanFolderModels);
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
        /// 添加空吧监控文件夹地址
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
            cleanDeathSetting.CleanFolders = new List<StructCleanFolder>();
            foreach (CleanFolderModel cleanFolderModel in CleanFolders)
            {
                if (string.IsNullOrWhiteSpace(cleanFolderModel.CleanFolderPath)
                    || newList.Any(n => n.CleanFolderPath.Equals(cleanFolderModel.CleanFolderPath)))
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
            SettingUtility.SaveTSetting(cleanDeathSetting);
        }

        /// <summary>
        /// 拖动exe或者lnk文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void AppFolderItemDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (files == null)
                {
                    InfoMessageShow("请拖放一个或多个文件夹。");
                    return;
                }
                foreach (string path in files)
                {
                    //将每一个文件夹都添加到datagrid中
                    if (Directory.Exists(path))
                    {
                        CleanFolderModel cleanFolderModel = new CleanFolderModel();
                        cleanFolderModel.CleanFolderPath = path;
                        cleanFolderModel.IsEnable = true;
                        CleanFolders.Add(cleanFolderModel);
                    }
                }
            }
        }

        #endregion

        #region 应用选择

        private ObservableCollection<AppRegistryModel> _allAppNames;

        /// <summary>
        /// 注册表中所有应用名称
        /// </summary>
        public ObservableCollection<AppRegistryModel> AllAppInfos
        {
            get { return _allAppNames; }
            set
            {
                _allAppNames = value;
                NotifyOfPropertyChange(() => AllAppInfos);
            }
        }

        private AppRegistryModel _selectedAppInfo;

        /// <summary>
        /// 选中的软件名称
        /// </summary>
        public AppRegistryModel SelectedAppInfo
        {
            get { return _selectedAppInfo; }
            set
            {
                _selectedAppInfo = value;
                NotifyOfPropertyChange(() => SelectedAppInfo);
            }
        }

        private ObservableCollection<CleanAppModel> _cleanAppInfos = new ObservableCollection<CleanAppModel>();

        /// <summary>
        /// 用户选择的app列表
        /// </summary>
        public ObservableCollection<CleanAppModel> CleanAppInfos
        {
            get { return _cleanAppInfos; }
            set
            {
                _cleanAppInfos = value;
                NotifyOfPropertyChange(() => CleanAppInfos);
            }
        }

        private CleanAppModel _selectCleanAppInfo;

        /// <summary>
        /// 选中的待清理的app信息
        /// </summary>
        public CleanAppModel SelectCleanAppInfo
        {
            get { return _selectCleanAppInfo; }
            set
            {
                _selectCleanAppInfo = value;
                NotifyOfPropertyChange(() => SelectCleanAppInfo);
            }
        }


        /// <summary>
        /// 获取本机电脑上所有应用名称
        /// </summary>
        private void GetAllAppNames()
        {
            GetAllAppNamesByRegistry();
            //GetSomeAppNotInRegistry();
        }

        /// <summary>
        /// 获取一些不在注册表中的app，完全写死,从配置中读取，并暴露给用户，用户可自定义
        /// </summary>
        private void GetSomeAppNotInRegistry()
        {
            SystemSetting systemSetting = SettingUtility.GetTSetting<SystemSetting>();
            if (systemSetting == null)
            {
                return;
            }
            if (AllAppInfos == null)
            {
                AllAppInfos = new ObservableCollection<AppRegistryModel>();
            }
            foreach (AppSetttingStruct appSetttingStruct in systemSetting.WindowApps)
            {
                if (AllAppInfos.Any(a => a.AppDisplayName == appSetttingStruct.AppDisplayName))
                {
                    continue;
                }
                if (Directory.Exists(appSetttingStruct.AppFilePath))
                {
                    AppRegistryModel appRegistryModel = new AppRegistryModel();
                    appRegistryModel.AppExePath = appSetttingStruct.AppFilePath;
                    appRegistryModel.AppExeName = appSetttingStruct.AppExeName;
                    appRegistryModel.AppDisplayName = appSetttingStruct.AppDisplayName;
                    AllAppInfos.Add(appRegistryModel);
                }
            }
        }

        /// <summary>
        /// 通过注册表获取所有已安装应用程序数据
        /// </summary>
        private void GetAllAppNamesByRegistry()
        {
            List<AppRegistryModel> installedApps = new List<AppRegistryModel>();

            // 遍历32位应用程序的注册表路径
            string keyPath32Bit = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            GetInstalledAppsFromRegistry(installedApps, RegistryView.Registry32, keyPath32Bit);
            // 遍历64位应用程序的注册表路径
            GetInstalledAppsFromRegistry(installedApps, RegistryView.Registry64, keyPath32Bit);

            //可能一些特殊应用存在如下位置
            //string keyPath64Bit = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
            //GetInstalledAppsFromRegistry(installedApps, RegistryView.Registry32, keyPath64Bit);
            //GetInstalledAppsFromRegistry(installedApps, RegistryView.Registry64, keyPath64Bit);

            installedApps = installedApps.OrderBy(i => i.AppDisplayName).ToList();
            AllAppInfos = new ObservableCollection<AppRegistryModel>(installedApps);
        }

        private void GetInstalledAppsFromRegistry(List<AppRegistryModel> installedApps, RegistryView registryView, string keyPath)
        {
            using (RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView))
            using (RegistryKey subKey = key.OpenSubKey(keyPath))
            {
                if (subKey != null)
                {
                    foreach (string subKeyName in subKey.GetSubKeyNames())
                    {
                        using (RegistryKey appKey = subKey.OpenSubKey(subKeyName))
                        {
                            string displayName = appKey.GetValue("DisplayName") as string;

                            //if (displayName != null && displayName.Contains("搜狗"))
                            //{

                            //}
                            //if (subKeyName != null && subKeyName.Contains("Moz"))
                            //{ 

                            //}

                            //foreach (string valueName in appKey.GetValueNames())
                            //{
                            //    string valueData = appKey.GetValue(valueName) as string;
                            //    Console.WriteLine("启动项名称：" + valueName);
                            //    Console.WriteLine("应用程序路径：" + valueData);
                            //}

                            // 排除系统应用 systemComponent == 1
                            int systemComponent = (int)appKey.GetValue("SystemComponent", 0);
                            if (systemComponent == 1 || string.IsNullOrWhiteSpace(displayName))
                            {
                                continue;
                            }
                            if (!installedApps.Any(i => i.AppDisplayName.Equals(displayName)))
                            {
                                AppRegistryModel appRegistryModel = new AppRegistryModel();
                                appRegistryModel.AppDisplayName = displayName;
                                appRegistryModel.InstallLocation = appKey.GetValue("InstallLocation") as string;
                                appRegistryModel.AppExePath = appKey.GetValue("DisplayIcon") as string;
                                appRegistryModel.UnInstallString = appKey.GetValue("UninstallString") as string;
                                if (!string.IsNullOrWhiteSpace(appRegistryModel.AppExePath) && appRegistryModel.AppExePath.Contains("\\"))
                                {
                                    string exePath = appRegistryModel.AppExePath;
                                    if (appRegistryModel.AppExePath.EndsWith(".exe"))
                                    {
                                        exePath = exePath.Split('\\').Last();
                                    }
                                    else
                                    {
                                        string[] exePaths = exePath.Split('\\');
                                        exePath = exePaths[exePaths.Length - 2];
                                    }
                                    appRegistryModel.AppExeName = exePath;
                                }
                                if (string.IsNullOrWhiteSpace(appRegistryModel.InstallLocation))
                                {
                                    appRegistryModel.InstallLocation = Path.GetDirectoryName(appRegistryModel.AppExePath);
                                }
                                installedApps.Add(appRegistryModel);
                            }
                        }
                    }
                }
            }
            using (RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, registryView))
            using (RegistryKey subKey = key.OpenSubKey(keyPath))
            {
                if (subKey != null)
                {
                    foreach (string subKeyName in subKey.GetSubKeyNames())
                    {
                        using (RegistryKey appKey = subKey.OpenSubKey(subKeyName))
                        {
                            string displayName = appKey.GetValue("DisplayName") as string;
                            // 排除系统应用 systemComponent == 1
                            int systemComponent = (int)appKey.GetValue("SystemComponent", 0);
                            if (systemComponent == 1 || string.IsNullOrWhiteSpace(displayName))
                            {
                                continue;
                            }
                            if (!installedApps.Any(i => i.AppDisplayName.Equals(displayName)))
                            {
                                AppRegistryModel appRegistryModel = new AppRegistryModel();
                                appRegistryModel.AppDisplayName = displayName;
                                appRegistryModel.InstallLocation = appKey.GetValue("InstallLocation") as string;
                                appRegistryModel.AppExePath = appKey.GetValue("DisplayIcon") as string;
                                appRegistryModel.UnInstallString = appKey.GetValue("UninstallString") as string;
                                if (!string.IsNullOrWhiteSpace(appRegistryModel.AppExePath))
                                {
                                    string exePath = appRegistryModel.AppExePath;
                                    if (exePath.Contains("\\"))
                                    {
                                        exePath = exePath.Split('\\').Last();
                                    }
                                    appRegistryModel.AppExeName = exePath;
                                }
                                if (string.IsNullOrWhiteSpace(appRegistryModel.InstallLocation))
                                {
                                    appRegistryModel.InstallLocation = Path.GetDirectoryName(appRegistryModel.AppExePath);
                                }
                                installedApps.Add(appRegistryModel);
                            }
                        }
                    }
                }
            }

        }

        /// <summary>
        /// 删除目录,且数据不可恢复
        /// </summary>
        /// <param name="dir">要删除的目录</param>
        public void DeleteFolder(string dir, List<string> notDeleteFileList = null, List<string> notDeleteFolderList = null)
        {
            if (System.IO.Directory.Exists(dir))
            {
                DirectoryInfo dirRoot = new DirectoryInfo(dir);
                FileInfo[] files = dirRoot.GetFiles();
                foreach (var item in files)
                {
                    if (notDeleteFileList != null && notDeleteFileList.Contains(item.Name))
                    {
                        continue;
                    }
                    // 使用随机数据覆盖文件内容
                    OverwriteFileWithRandomData(item.FullName);
                    File.Delete(item.FullName);
                }

                DirectoryInfo[] dirs = dirRoot.GetDirectories();

                foreach (var item in dirs)
                {
                    if (notDeleteFolderList != null && notDeleteFolderList.Contains(item.Name))
                    {
                        continue;
                    }
                    DeleteFolder(item.FullName);
                    Directory.Delete(item.FullName);
                }
            }
        }

        /// <summary>
        /// 覆盖数据
        /// </summary>
        /// <param name="filePath"></param>
        private void OverwriteFileWithRandomData(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Write))
            {
                // 创建随机数据
                byte[] randomData = new byte[fs.Length];
                new RNGCryptoServiceProvider().GetBytes(randomData);
                // 覆盖文件内容
                fs.Seek(0, SeekOrigin.Begin);
                fs.Write(randomData, 0, randomData.Length);
            }
            TLog.Debug($"覆盖文件完成 {filePath}");
        }

        /// <summary>
        /// 添加软件信息
        /// </summary>
        public void AddAppInfoEvent()
        {
            if (SelectedAppInfo == null)
            {
                InfoMessageShow("请先选择应用");
                return;
            }
            if (CleanAppInfos == null)
            {
                CleanAppInfos = new ObservableCollection<CleanAppModel>();
            }
            if (CleanAppInfos.Any(c => c.AppDisplayName == SelectedAppInfo.AppDisplayName))
            {
                InfoMessageShow("已有相同应用在列表中");
                return;
            }
            if (SelectedAppInfo.AppDisplayName == "Mozilla Firefox")
            {
                InfoMessageShow("火狐浏览器可以使用其他浏览器的历史记录和收藏夹，请注意！");
            }
            CleanAppModel cleanAppModel = new CleanAppModel();
            cleanAppModel.AppDisplayName = SelectedAppInfo.AppDisplayName;
            cleanAppModel.AppExeName = SelectedAppInfo.AppExeName;
            cleanAppModel.AppExePath = string.IsNullOrWhiteSpace(SelectedAppInfo.InstallLocation) ? SelectedAppInfo.AppExePath : SelectedAppInfo.InstallLocation;
            CleanAppInfos.Add(cleanAppModel);
        }

        /// <summary>
        /// 手动添加要监控的软件
        /// </summary>
        /// <param name="cleanAppModel"></param>
        public void AddAppInfoEvent(CleanAppModel cleanAppModel)
        {
            if (cleanAppModel == null)
            {
                return;
            }
            if (CleanAppInfos == null)
            {
                CleanAppInfos = new ObservableCollection<CleanAppModel>();
            }
            if (CleanAppInfos.Any(c => c.AppDisplayName == cleanAppModel.AppDisplayName))
            {
                InfoMessageShow("已有相同应用在列表中");
                return;
            }

            CleanAppInfos.Add(cleanAppModel);
        }

        /// <summary>
        /// 手动添加软件信息
        /// </summary>
        public void HandAddAppInfoEvent()
        {
            AppInfoViewModel appInfoViewModel = new AppInfoViewModel();
            appInfoViewModel.AddAppInfo -= AddAppInfoEvent;
            appInfoViewModel.AddAppInfo += AddAppInfoEvent;
            _iWindowManager.ShowDialogAsync(appInfoViewModel);
        }

        /// <summary>
        /// 删除选中的守护软件项
        /// </summary>
        public void DeleteSelectedApp()
        {
            if (CleanAppInfos == null)
            {
                return;
            }
            CleanAppInfos.Remove(SelectCleanAppInfo);
            if (CleanAppInfos.Count > 0)
            {
                SelectCleanAppInfo = CleanAppInfos[0];
            }

        }

        /// <summary>
        /// 保存守护app信息到配置
        /// </summary>
        public void SaveCleanAppsEvent()
        {
            CleanDeathSetting cleanDeathSetting = SettingUtility.GetTSetting<CleanDeathSetting>();
            if (cleanDeathSetting == null)
            {
                cleanDeathSetting = new CleanDeathSetting();
            }
            List<CleanAppModel> newList = new List<CleanAppModel>();
            cleanDeathSetting.CleanApps = new List<AppSetttingStruct>();
            foreach (CleanAppModel cleanAppModel in CleanAppInfos)
            {
                if (string.IsNullOrWhiteSpace(cleanAppModel.AppExePath)
                    || newList.Any(n => n.AppDisplayName.Equals(cleanAppModel.AppDisplayName)))
                {
                    continue;
                }
                newList.Add(cleanAppModel);
                AppSetttingStruct structCleanApp = new AppSetttingStruct();
                structCleanApp.AppDisplayName = cleanAppModel.AppDisplayName;
                structCleanApp.AppExeName = cleanAppModel.AppExeName;
                structCleanApp.AppFilePath = cleanAppModel.AppExePath;
                structCleanApp.IsEnable = cleanAppModel.IsEnable;
                cleanDeathSetting.CleanApps.Add(structCleanApp);
            }
            CleanAppInfos = new ObservableCollection<CleanAppModel>(newList);
            SettingUtility.SaveTSetting(cleanDeathSetting);
        }

        /// <summary>
        /// 加载需要守护的app信息
        /// </summary>
        public void CleanAppsLoaded()
        {
            CleanDeathSetting cleanDeathSetting = SettingUtility.GetTSetting<CleanDeathSetting>();
            if (cleanDeathSetting == null)
            {
                cleanDeathSetting = new CleanDeathSetting();
            }

            List<CleanAppModel> cleanFolderModels = new List<CleanAppModel>();
            foreach (AppSetttingStruct structCleanFolder in cleanDeathSetting.CleanApps)
            {
                CleanAppModel cleanAppModel = new CleanAppModel();
                cleanAppModel.AppExePath = structCleanFolder.AppFilePath;
                cleanAppModel.AppExeName = structCleanFolder.AppExeName;
                cleanAppModel.IsEnable = structCleanFolder.IsEnable;
                cleanAppModel.AppDisplayName = structCleanFolder.AppDisplayName;
                cleanFolderModels.Add(cleanAppModel);
            }

            CleanAppInfos = new ObservableCollection<CleanAppModel>(cleanFolderModels);
            if (CleanAppInfos != null && CleanAppInfos.Count > 0)
            {
                SelectCleanAppInfo = CleanAppInfos[0];
            }
        }

        public void AppAppItemDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (files == null)
                {
                    InfoMessageShow("请拖放.lnk或.exe文件。");
                }
                bool isAllPathError = true;
                // 检查是否是.lnk文件
                foreach (string exePath in files)
                {

                    if (!exePath.EndsWith(".exe") && !exePath.EndsWith(".lnk"))
                    {
                        continue;
                    }
                    if (isAllPathError)
                    {
                        isAllPathError = false;
                    }
                    SetAppInfoByPath(exePath);
                }
            }
        }

        /// <summary>
        /// 通过选择的文件来设置app对象信息
        /// </summary>
        /// <param name="selectedFilePath"></param>
        private void SetAppInfoByPath(string selectedFilePath)
        {
            if (!string.IsNullOrWhiteSpace(selectedFilePath))
            {
                if (selectedFilePath.EndsWith(".lnk"))
                {
                    selectedFilePath = PathHelp.GetShortcutTarget(selectedFilePath);
                }

                string exeName = selectedFilePath;
                if (exeName.Contains("\\"))
                {
                    exeName = exeName.Split('\\').Last();
                }
                CleanAppModel cleanAppModel = new CleanAppModel();
                cleanAppModel.AppExePath = Path.GetDirectoryName(selectedFilePath);
                cleanAppModel.AppExeName = exeName.Replace(".exe", "");
                cleanAppModel.AppDisplayName = exeName.Replace(".exe", "");
                AddAppInfoEvent(cleanAppModel);
            }
        }

        #endregion

        #region 主程序逻辑

        private Visibility _isBusyModel = Visibility.Collapsed;

        public Visibility BusyBorderShow
        {
            get { return _isBusyModel; }
            set
            {
                _isBusyModel = value;
                NotifyOfPropertyChange(() => BusyBorderShow);
            }
        }


        public void ViewModelLoaded()
        {
            CleanFoldersLoaded();
            GetAllAppNames();
            CleanAppsLoaded();
        }

        public void TabSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source.GetType() != typeof(TabControl))
            {
                return;
            }
            if (e.AddedItems != null && e.AddedItems.Count > 0 && e.AddedItems[0].GetType().Name == "TabItem")
            {
                //System.Windows.Controls.TabItem tabAddItem = e.AddedItems[0] as System.Windows.Controls.TabItem;
                //if (tabAddItem.Header.ToString() == "软件设置")
                //{
                //    InfoMessageShow("仅支持64位系统");
                //}
            }
        }

        /// <summary>
        /// 开始守护底裤
        /// </summary>
        public async void StartGuard()
        {
            if (MessageBoxResult.No == System.Windows.MessageBox.Show("已保存所有设置？", "询问", MessageBoxButton.YesNo))
            {
                return;
            }
            BusyBorderShow = Visibility.Visible;
            await Task.Run(() =>
            {
                RestratHelp.RunRestartTools(true);
                //更新检测时间
                CleanDeathSetting cleanDeathSetting = SettingUtility.GetTSetting<CleanDeathSetting>();
                cleanDeathSetting.MaxTimeOutDay = MaxTimeOutDay;
                cleanDeathSetting.NeedCleanTime = DateTime.Now.AddDays(MaxTimeOutDay);
                SettingUtility.SaveTSetting(cleanDeathSetting);
                ServiceUtility.StartService();
                //初始化服务客户端
                ServiceClientUtility.InitClient();
                Thread.Sleep(1000);
                this.TryCloseAsync();
            });
            BusyBorderShow = Visibility.Collapsed;

        }

        public void ViewModelClosed()
        {
            //System.Windows.MessageBox.Show("未开启守护", "通知", MessageBoxButton.OK);
        }

        #endregion  

        #region message

        public void InfoMessageShow(string message)
        {
            Growl.Info(message, "InfoMessage");
        }
        public void ErrorMessageShow(string message)
        {
            Growl.Error(message, "ErrorMessage");
            return;
        }
        public void ClearMessageShow()
        {
            HandyControl.Controls.Growl.Clear("InfoMessage");
            HandyControl.Controls.Growl.Clear("ErrorMessage");
            return;
        }

        #endregion
    }
}
