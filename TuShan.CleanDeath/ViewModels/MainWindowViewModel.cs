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
using TuShan.CleanDeath.Views;
using TabControl = System.Windows.Controls.TabControl;

namespace TuShan.CleanDeath.ViewModels
{
    public class MainWindowViewModel : Caliburn.Micro.Screen
    {
        private readonly IWindowManager _iWindowManager;

        private CleanDeathSetting _cleanDeathSetting;
        public MainWindowViewModel()
        {
            _iWindowManager = IoC.Get<IWindowManager>();
            _cleanDeathSetting = SettingUtility.GetTSetting<CleanDeathSetting>();
        }

        #region 文件夹设置相关

        private ObservableCollection<CleanFolderModel> _cleanFolders = new ObservableCollection<CleanFolderModel>();

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

        public CleanFolderModel SelectedCleanFolderItem
        {
            get { return _selectedCleanFolderItem; }
            set
            {
                _selectedCleanFolderItem = value;
                NotifyOfPropertyChange(() => SelectedCleanFolderItem);
            }
        }

        private int _maxTimeOutDay = 1;

        public int MaxTimeOutDay
        {
            get { return _maxTimeOutDay; }
            set
            {
                _maxTimeOutDay = value;
                NotifyOfPropertyChange(() => MaxTimeOutDay);
            }
        }

        public void CleanFoldersLoaded()
        {
            if (_cleanDeathSetting == null)
            {
                _cleanDeathSetting = SettingUtility.GetTSetting<CleanDeathSetting>();
            }

            List<CleanFolderModel> cleanFolderModels = new List<CleanFolderModel>();
            foreach (StructCleanFolder structCleanFolder in _cleanDeathSetting.CleanFolders)
            {
                CleanFolderModel cleanFolderModel = new CleanFolderModel();
                cleanFolderModel.CleanFolderPath = structCleanFolder.FolderPath;
                cleanFolderModel.IsEnable = structCleanFolder.IsEnable;
                cleanFolderModels.Add(cleanFolderModel);
            }

            CleanFolders = new ObservableCollection<CleanFolderModel>(cleanFolderModels);
            DataWriteTime = _cleanDeathSetting.WriteTime;
        }

        public void SelectedFolder()
        {
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog
            {
                Description = "选择文件夹",
                UseDescriptionForTitle = true
            };
            if (dialog.ShowDialog() == true)
            {
                if (SelectedCleanFolderItem != null)
                {
                    SelectedCleanFolderItem.CleanFolderPath = dialog.SelectedPath;
                }
            }
        }

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

        public void DeleteSelectedFolder()
        {
            if (CleanFolders == null)
            {
                return;
            }
            CleanFolders.Remove(SelectedCleanFolderItem);

        }

        public void AddCleanFolderEvent()
        {
            CleanFolderModel cleanFolderModel = new CleanFolderModel();
            if (CleanFolders == null)
            {
                CleanFolders = new ObservableCollection<CleanFolderModel>();
            }
            CleanFolders.Add(cleanFolderModel);
        }

        public void SaveCleanFolderEvent()
        {
            if (_cleanDeathSetting == null)
            {
                _cleanDeathSetting = SettingUtility.GetTSetting<CleanDeathSetting>();
            }
            List<CleanFolderModel> newList = new List<CleanFolderModel>();
            _cleanDeathSetting.CleanFolders = new List<StructCleanFolder>();
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
                _cleanDeathSetting.CleanFolders.Add(structCleanFolder);
            }
            CleanFolders = new ObservableCollection<CleanFolderModel>(newList);
            SettingUtility.SaveTSetting(_cleanDeathSetting);
        }

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

        public CleanAppModel SelectCleanAppInfo
        {
            get { return _selectCleanAppInfo; }
            set
            {
                _selectCleanAppInfo = value;
                NotifyOfPropertyChange(() => SelectCleanAppInfo);
            }
        }


        private void GetAllAppNames()
        {
            GetAllAppNamesByRegistry();
        }

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

        private void GetAllAppNamesByRegistry()
        {
            List<AppRegistryModel> installedApps = new List<AppRegistryModel>();

            string keyPath32Bit = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            GetInstalledAppsFromRegistry(installedApps, RegistryView.Registry32, keyPath32Bit);
            GetInstalledAppsFromRegistry(installedApps, RegistryView.Registry64, keyPath32Bit);

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

        public void HandAddAppInfoEvent()
        {
            AppInfoViewModel appInfoViewModel = new AppInfoViewModel();
            appInfoViewModel.AddAppInfo -= AddAppInfoEvent;
            appInfoViewModel.AddAppInfo += AddAppInfoEvent;
            _iWindowManager.ShowDialogAsync(appInfoViewModel);
        }

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

        public void SaveCleanAppsEvent()
        {
            if (_cleanDeathSetting == null)
            {
                _cleanDeathSetting = new CleanDeathSetting();
            }
            List<CleanAppModel> newList = new List<CleanAppModel>();
            _cleanDeathSetting.CleanApps = new List<AppSetttingStruct>();
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
                _cleanDeathSetting.CleanApps.Add(structCleanApp);
            }
            CleanAppInfos = new ObservableCollection<CleanAppModel>(newList);
            SettingUtility.SaveTSetting(_cleanDeathSetting);
        }

        public void CleanAppsLoaded()
        {
            if (_cleanDeathSetting == null)
            {
                _cleanDeathSetting = SettingUtility.GetTSetting<CleanDeathSetting>();
            }

            List<CleanAppModel> cleanFolderModels = new List<CleanAppModel>();
            foreach (AppSetttingStruct structCleanFolder in _cleanDeathSetting.CleanApps)
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
            }
        }

        public async void StartGuard()
        {
            if (MessageBoxResult.No == MyMessageBox.Show($"已保存所有设置？\r\n*将每隔{MaxTimeOutDay}天进行用户活跃检查", CleanDeath.Views.ButtonType.YesNo, MessageType.Question))
            {
                return;
            }
            BusyBorderShow = Visibility.Visible;
            await Task.Run(() =>
            {
                ServiceUtility.StopService();
                Thread.Sleep(1000);
                RestratHelp.RunRestartTools(true);
                _cleanDeathSetting.MaxTimeOutDay = MaxTimeOutDay;
                _cleanDeathSetting.NeedCleanTime = DateTime.Now.AddDays(MaxTimeOutDay);
                SettingUtility.SaveTSetting(_cleanDeathSetting);
                ServiceUtility.StartService();
                ServiceClientUtility.InitClient();
                Thread.Sleep(1000);
                this.TryCloseAsync();
            });
            BusyBorderShow = Visibility.Collapsed;

        }

        public void ViewModelClosed()
        {
            if (!ServiceUtility.IsServiceRun())
            {
                MyMessageBox.Show("未开启守护", CleanDeath.Views.ButtonType.OK, MessageType.Info);
            }
        }

        #region 其他功能

        private int _dataWriteTime = 1;

        public int DataWriteTime
        {
            get { return _dataWriteTime; }
            set
            {
                _dataWriteTime = value;
                NotifyOfPropertyChange(() => DataWriteTime);
            }
        }


        public void DataWriteTimeLostFocus()
        {
            _cleanDeathSetting.WriteTime = DataWriteTime;
            SettingUtility.SaveTSetting(_cleanDeathSetting);
        }

        public async void StopCleanDeath()
        {
            if (!ServiceUtility.IsServiceRun())
            {
                InfoMessageShow("未开始守护");
                return;
            }
            BusyBorderShow = Visibility.Visible;
            await Task.Run(() =>
            {
                ServiceUtility.StopService();
                Thread.Sleep(20);
                RestratHelp.RunRestartTools(false);
            });
            BusyBorderShow = Visibility.Collapsed;
            InfoMessageShow("已停止守护");
        }

        public async void StartGuardNow()
        {
            if (MessageBoxResult.Cancel == MyMessageBox.Show($"已保存所有设置？\r\n将会立刻开始删除，请确认", CleanDeath.Views.ButtonType.OKCancel, MessageType.Question))
            {
                return;
            }
            if (MessageBoxResult.Cancel == MyMessageBox.Show($"已保存所有设置？\r\n将会立刻开始删除，请确认", CleanDeath.Views.ButtonType.OKCancel, MessageType.Question))
            {
                return;
            }

            BusyBorderShow = Visibility.Visible;
            await Task.Run(() =>
            {
                StartClean();
            });
            BusyBorderShow = Visibility.Collapsed;
            InfoMessageShow("已完成清理");

        }

        #region 立即删除逻辑

        public static List<string> _NeedDeleteAppFolder = new List<string>();

        private void StartClean()
        {
            _NeedDeleteAppFolder = new List<string>();
            CleanFolder();
            CleanApp();
        }

        private void CleanFolder()
        {
            TLog.Debug("开始删除文件");
            foreach (StructCleanFolder structCleanFolder in _cleanDeathSetting.CleanFolders)
            {
                if (structCleanFolder.IsEnable)
                {
                    try
                    {
                        DeleteFolder(structCleanFolder.FolderPath);
                    }
                    catch (Exception ex)
                    {
                        TLog.Error("Delete file error :" + ex.Message);
                    }
                }
            }
        }

        private void CleanApp()
        {
            try
            {
                foreach (AppSetttingStruct structCleanFolder in _cleanDeathSetting.CleanApps)
                {
                    if (!structCleanFolder.IsEnable)
                    {
                        continue;
                    }
                    CloseProcessByName(structCleanFolder.AppExeName);
                }
                Thread.Sleep(100);
                DeleteAppRegistryByName();
                GetAppCacheFolderPath();

                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                TraverseDirectory(desktopPath, _cleanDeathSetting.CleanApps);
                string allDesktopPath = "C:\\Users\\Public\\Desktop";
                TraverseDirectory(allDesktopPath, _cleanDeathSetting.CleanApps);
                Thread.Sleep(100);
                DeleteLnkOnTask();
                Thread.Sleep(100);
                foreach (string path in _NeedDeleteAppFolder)
                {
                    DeleteFolder(path);
                }
            }
            catch (Exception ex)
            {
                TLog.Error($"CleanApp error {ex.Message}");
            }
        }

        private void DeleteLnkOnTask()
        {
            string exePath = $"{AppDomain.CurrentDomain.BaseDirectory}TuShan.DeleteTaskbarIcon.exe";
            Process myprocess = new Process();

            ProcessStartInfo startInfo = new ProcessStartInfo(exePath);
            myprocess.StartInfo = startInfo;
            myprocess.Start();
            myprocess.WaitForExit();

            if (myprocess.ExitCode != 1)
            {
                TLog.Error("删除任务栏快捷方式错误");
            }
        }

        void TraverseDirectory(string folderPath, List<AppSetttingStruct> cleanDeathSetting)
        {
            try
            {
                if (!Directory.Exists(folderPath))
                {
                    return;
                }
                string[] files = Directory.GetFiles(folderPath);
                foreach (string file in files)
                {
                    if (file.Contains(".lnk"))
                    {
                        string fileee = GetShortcutTarget(file);
                        if (cleanDeathSetting.Any(c => c.IsEnable &&( fileee.Contains(c.AppExeName) || fileee.Contains(c.AppDisplayName) || file.Contains(c.AppDisplayName))))
                        {
                            File.Delete(file);
                        }
                    }
                }
                string[] subdirectories = Directory.GetDirectories(folderPath);
                foreach (string subdirectory in subdirectories)
                {
                    TraverseDirectory(subdirectory, cleanDeathSetting);
                }
            }
            catch (Exception ex)
            {
            }
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

        private void GetAppCacheFolderPath()
        {
            _NeedDeleteAppFolder = new List<string>();
            Stopwatch stopwatch = Stopwatch.StartNew();
            string localFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string parentDirectory = Directory.GetParent(localFolderPath).FullName;
            string roamingFolderPath = Path.Combine(parentDirectory, "Roaming");
            string localLowFolderPath = Path.Combine(parentDirectory, "LocalLow");
            if (_cleanDeathSetting == null)
            {
                _cleanDeathSetting = SettingUtility.GetTSetting<CleanDeathSetting>();
            }
            List<string> exeNameLists = new List<string>();
            foreach (AppSetttingStruct structCleanFolder in _cleanDeathSetting.CleanApps)
            {
                if (!structCleanFolder.IsEnable)
                {
                    continue;
                }
                if (structCleanFolder.AppFilePath.Contains("."))
                {
                    structCleanFolder.AppFilePath = Directory.GetParent(structCleanFolder.AppFilePath).FullName;
                }
                if (!_NeedDeleteAppFolder.Contains(structCleanFolder.AppFilePath))
                {
                    _NeedDeleteAppFolder.Add(structCleanFolder.AppFilePath);
                    exeNameLists.Add(structCleanFolder.AppExeName);
                    if (structCleanFolder.AppExeName.Contains(" "))
                    {
                        exeNameLists.Add(structCleanFolder.AppExeName.Split(' ').Last());
                    }
                }
            }

            Task task1 = Task.Run(() =>
            {
                GetDeleteFloderByExeName(localFolderPath, exeNameLists, ref _NeedDeleteAppFolder);
            });
            Task task2 = Task.Run(() =>
            {
                GetDeleteFloderByExeName(roamingFolderPath, exeNameLists, ref _NeedDeleteAppFolder);
            });
            Task task3 = Task.Run(() =>
            {
                GetDeleteFloderByExeName(localLowFolderPath, exeNameLists, ref _NeedDeleteAppFolder);
            });
            Task[] tasks = new Task[] { task1, task2, task3 };
            Task.WaitAll(tasks);
            stopwatch.Stop();
        }

        private void GetDeleteFloderByExeName(string srcPath, List<string> exeNameLists, ref List<string> needDeleteFolder)
        {
            try
            {
                DirectoryInfo dirRoot = new DirectoryInfo(srcPath);
                if (dirRoot == null)
                {
                    return;
                }
                DirectoryInfo[] dirs = dirRoot.GetDirectories();
                if (dirs == null)
                {
                    return;
                }
                foreach (var item in dirs)
                {
                    if (exeNameLists.Contains(item.Name))
                    {
                        if (!needDeleteFolder.Contains(item.FullName))
                        {
                            needDeleteFolder.Add(item.FullName);
                        }
                    }
                    GetDeleteFloderByExeName(item.FullName, exeNameLists, ref needDeleteFolder);
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }

        private void CloseProcessByName(string peocessName)
        {
            Process[] processes = Process.GetProcessesByName(peocessName);

            if (processes.Length > 0)
            {
                foreach (Process process in processes)
                {
                    process.Kill();
                }
            }
            else
            {
                List<Process> processList = Process.GetProcesses().ToList();
                if (processList != null)
                {
                    string temppeocessName = peocessName.ToLower();
                    Process[] processRun = processList.Where(p => !string.IsNullOrWhiteSpace(p.ProcessName) && temppeocessName.Contains(p.ProcessName))?.ToArray();
                    if (processRun != null)
                    {
                        foreach (Process process in processRun)
                        {
                            process.Kill();
                        }
                    }
                }
            }
        }

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

        private void OverwriteFileWithRandomData(string filePath)
        {
            int writeTime = _cleanDeathSetting == null ? 3 : _cleanDeathSetting.WriteTime;
            for (int i = 0; i < writeTime; i++)
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Write))
                {
                    byte[] randomData = new byte[fs.Length];
                    new RNGCryptoServiceProvider().GetBytes(randomData);
                    fs.Seek(0, SeekOrigin.Begin);
                    fs.Write(randomData, 0, randomData.Length);
                }
            }
        }

        private void DeleteAppRegistryByName()
        {
            if (_cleanDeathSetting == null)
            {
                return;
            }
            string keyPath32Bit = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            DeleteAppsFromRegistry(_cleanDeathSetting, RegistryView.Registry32, keyPath32Bit);

            DeleteAppsFromRegistry(_cleanDeathSetting, RegistryView.Registry64, keyPath32Bit);

            string keyPath64Bit = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
            DeleteAppsFromRegistry(_cleanDeathSetting, RegistryView.Registry32, keyPath64Bit);
            DeleteAppsFromRegistry(_cleanDeathSetting, RegistryView.Registry64, keyPath64Bit);
        }

        private void DeleteAppsFromRegistry(CleanDeathSetting cleanDeathSetting, RegistryView registryView, string keyPath)
        {
            string LocalMachineName = "HKEY_LOCAL_MACHINE\\";
            List<string> needDeletelist = new List<string>();
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
                            if (!string.IsNullOrWhiteSpace(displayName) && cleanDeathSetting.CleanApps.Any(c => c.IsEnable &&  c.AppDisplayName != null && displayName.Contains(c.AppDisplayName)))
                            {
                                needDeletelist.Add(appKey.Name.Replace(LocalMachineName, ""));
                            }

                        }
                    }
                    foreach (string appKey in needDeletelist)
                    {
                        try
                        {
                            RegistryKey deletekey = Registry.LocalMachine.OpenSubKey(appKey, true);    

                            if (deletekey != null)
                            {
                                Registry.LocalMachine.DeleteSubKeyTree(appKey);
                            }
                            else
                            {
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
            }
            needDeletelist = new List<string>();
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
                            if (!string.IsNullOrWhiteSpace(displayName) && cleanDeathSetting.CleanApps.Any(c => c.IsEnable && c.AppDisplayName != null && displayName.Contains(c.AppDisplayName)))
                            {
                                needDeletelist.Add(appKey.Name.Replace(LocalMachineName, ""));
                            }
                        }
                    }
                    foreach (string appKey in needDeletelist)
                    {
                        try
                        {
                            RegistryKey deletekey = Registry.LocalMachine.OpenSubKey(appKey, true);    

                            if (deletekey != null)
                            {
                                Registry.LocalMachine.DeleteSubKeyTree(appKey);
                            }
                            else
                            {
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
            }

        }


        #endregion

        #endregion

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
