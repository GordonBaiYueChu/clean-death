﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
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

        public MainWindowViewModel()
        {
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

        #endregion

        #region 应用选择

        private ObservableCollection<AppRegistryModel> _allAppNames;

        /// <summary>
        /// 注册表中所有应用名称
        /// </summary>
        public ObservableCollection<AppRegistryModel> AllAppNames
        {
            get { return _allAppNames; }
            set
            {
                _allAppNames = value;
                NotifyOfPropertyChange(() => AllAppNames);
            }
        }

        private string _selectedAppName;

        /// <summary>
        /// 选中的软件名称
        /// </summary>
        public string SelectedAppName
        {
            get { return _selectedAppName; }
            set
            {
                _selectedAppName = value;
                NotifyOfPropertyChange(() => SelectedAppName);
            }
        }

        /// <summary>
        /// 获取本机电脑上所有应用名称
        /// </summary>
        private void GetAllAppNames()
        {
            GetAllAppNamesByRegistry();
            GetSomeAppNotInRegistry();
        }

        /// <summary>
        /// 获取一些不在注册表中的app，完全写死,从配置中读取，并暴露给用户，用户可自定义
        /// </summary>
        private void GetSomeAppNotInRegistry()
        {
            //火狐浏览器

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
            string keyPath64Bit = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
            GetInstalledAppsFromRegistry(installedApps, RegistryView.Registry64, keyPath64Bit);

            installedApps = installedApps.OrderBy(i => i.AppDisplayName).ToList();
            AllAppNames = new ObservableCollection<AppRegistryModel>(installedApps);
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
                            if (displayName.ToLower().Contains("chrome"))
                            {
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

        private void ExecuteUninstallCommand(string command)
        {
            if (!string.IsNullOrEmpty(command))
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process process = new Process
                {
                    StartInfo = psi
                };

                process.Start();

                // 执行卸载命令
                process.StandardInput.WriteLine(command);
                process.WaitForExit();
            }
        }

        public void DeleteAppCache(string appName)
        {
            string basePath = "C:\\Users";
            if (System.IO.Directory.Exists(basePath))
            {
                DirectoryInfo dirRoot = new DirectoryInfo(basePath);
                DirectoryInfo[] dirs = dirRoot.GetDirectories();
                foreach (DirectoryInfo item in dirs)
                {
                    if (item.FullName.Contains("Default") || item.FullName.Contains("Public"))
                    {
                        continue;
                    }
                    string appCachePath = Path.Combine(item.FullName, "AppDara", "Roaming");

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

        #endregion

        public void ViewModelLoaded()
        {
            CleanFoldersLoaded();
            GetAllAppNames();
        }

        public void TabSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source.GetType() != typeof(TabControl))
            {
                return;
            }
            if (e.AddedItems != null && e.AddedItems.Count > 0 && e.AddedItems[0].GetType().Name == "TabItem")
            {
                System.Windows.Controls.TabItem tabAddItem = e.AddedItems[0] as System.Windows.Controls.TabItem;
                if (tabAddItem.Header.ToString() == "软件设置")
                {
                    InfoMessageShow("仅支持64位系统");
                }
            }
        }

        /// <summary>
        /// 开始守护底裤
        /// </summary>
        public void StartGuard()
        {
            //更新检测时间
            CleanDeathSetting cleanDeathSetting = SettingUtility.GetTSetting<CleanDeathSetting>();
            cleanDeathSetting.MaxTimeOutDay = MaxTimeOutDay;
            cleanDeathSetting.NeedCleanTime = DateTime.Now.AddDays(MaxTimeOutDay);
            SettingUtility.SaveTSetting(cleanDeathSetting);

            Task.Run(() =>
            {
                ServiceUtility.StartService();
                //初始化服务客户端
                ServiceClientUtility.InitClient();
                Thread.Sleep(1000);
                this.TryCloseAsync();
            });

        }


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
