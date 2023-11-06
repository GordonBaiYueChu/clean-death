using NetMQ.Sockets;
using NetMQ;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TuShan.BountyHunterDream.Logger;
using TuShan.BountyHunterDream.Setting.Common;
using TuShan.CleanDeath.Service.Struct;
using Microsoft.Win32;
using System.ServiceProcess;
using TuShan.BountyHunterDream.Setting.Setting;
using TuShan.BountyHunterDream.Setting;
using System.Threading;
using TuShan.BountyHunterDream.Setting.Struct;
using System.IO;
using System.Security.Cryptography;
using System.Reflection;
using TuShan.CleanDeath.Service.Utility;

namespace TuShan.CleanDeath.Service
{
    public class CleanDeathServer
    {
        #region 服务基本配置

        private string _ip = "127.0.0.1";
        private int _port = 45558;
        private int _publishPort = 45559;
        private bool _do = false;
        private TimeSpan _ts = new TimeSpan(0, 0, 5);
        private PublisherSocket _pubSocket = new PublisherSocket();

        #endregion

        private bool _isRun = false;

        public CleanDeathServer()
        {
            Begin();
        }

        private void Begin()
        {
            Task.Run(() =>
            {
                _do = true;
                using (NetMQSocket serverSocket = new ResponseSocket())
                {
                    serverSocket.Bind($"tcp://{_ip}:{_port}");
                    while (_do)
                    {
                        try
                        {
                            string requestStr = "";
                            bool ret = serverSocket.TryReceiveFrameString(_ts, out requestStr);
                            if (ret)
                            {
                                if (string.IsNullOrWhiteSpace(requestStr))
                                {
                                    continue;
                                }
                                var request = JsonUtil.ToObject<RequestStruct>(requestStr);
                                ResponseStruct response = GetResponseStruct(request);
                                var responseStr = JsonUtil.ToJson(response);
                                serverSocket.TrySendFrame(responseStr);
                            }
                        }
                        catch (Exception ex)
                        {
                            TLog.Error(ex.ToString());
                        }
                    }
                }
            });
            _pubSocket.Options.SendHighWatermark = 1000;
            _pubSocket.Bind($"tcp://{_ip}:{_publishPort}");
        }


        private ResponseStruct GetResponseStruct(RequestStruct request)
        {
            ResponseStruct response = new ResponseStruct { ProcessID = request.ProcessPath };
            if (request.SocketEnum == SocketEnum.ConnectServer)
            {
                response.ResultType = typeof(bool);
                response.ResultsJson = JsonUtil.ToJson(true);
                Environment.SetEnvironmentVariable("USERPROFILE", request.USERPROFILEEnvironmentPath, EnvironmentVariableTarget.Process);
                processPath = request.ProcessPath;
                LoopRunClean();
                _do = false;
            }
            return response;
        }

        private string processPath = string.Empty;

        public void SessionChanged(SessionChangeReason sessionChangeReason)
        {
            switch (sessionChangeReason)
            {
                case SessionChangeReason.SessionLogon:

                    break;
                case SessionChangeReason.SessionLogoff:

                    break;
                case SessionChangeReason.RemoteConnect:

                    break;
                case SessionChangeReason.RemoteDisconnect:

                    break;
                case SessionChangeReason.SessionLock:
                    UpdateCleanTime();
                    UpdateRunStatus(true);
                    break;
                case SessionChangeReason.SessionUnlock:
                    UpdateCleanTime();
                    UpdateRunStatus(false);
                    break;
                default:
                    break;
            }
        }

        private void UpdateCleanTime()
        {
            CleanDeathSetting cleanDeathSetting = ReadCleanDeathSetting();
            cleanDeathSetting.NeedCleanTime = DateTime.Now.AddDays(cleanDeathSetting.MaxTimeOutDay);
            SaveCleanDeathSetting(cleanDeathSetting);
        }

        private void UpdateRunStatus(bool isRun)
        {
            _isRun = isRun;
        }

        private void LoopRunClean()
        {
            Task.Run(() =>
            {
                CleanDeathSetting cleanDeathSetting = ReadCleanDeathSetting();
                DateTime dateTime1 = DateTime.Now;
                while (true)
                {
                    if (!_isRun)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }
                    TLog.Debug("开始守护");
                    if (DateTime.Now > cleanDeathSetting.NeedCleanTime)
                    {
                        TLog.Debug("删除你的宝贝们，不可恢复");
                        StartClean();
                        TLog.Debug("已经删除了你的宝贝们，不可恢复");
                        _isRun = false;
                        ServiceUtility.StopService();
                        return;
                    }
                }
            });
        }

        private void StartClean()
        {
            CleanFolder();
            CleanApp();
        }

        private void CleanFolder()
        {
            TLog.Debug("开始删除文件");
            CleanDeathSetting cleanDeathSetting = ReadCleanDeathSetting();
            foreach (StructCleanFolder structCleanFolder in cleanDeathSetting.CleanFolders)
            {
                if (!structCleanFolder.IsEnable)
                {
                    continue;
                }
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

        private void CleanApp()
        {
            try
            {
                CleanDeathSetting cleanDeathSetting = ReadCleanDeathSetting();
                foreach (AppSetttingStruct structCleanFolder in cleanDeathSetting.CleanApps)
                {
                    if (structCleanFolder.IsEnable)
                    {
                        CloseProcessByName(structCleanFolder.AppExeName);
                    }
                }
                Thread.Sleep(100);
                DeleteAppRegistryByName();
                GetAppCacheFolderPath();

                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                TraverseDirectory(desktopPath, cleanDeathSetting.CleanApps);
                string allDesktopPath = "C:\\Users\\Public\\Desktop";
                TraverseDirectory(allDesktopPath, cleanDeathSetting.CleanApps);
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

            }
        }

        private void DeleteLnkOnTask()
        {
            string exePath = $"{AppDomain.CurrentDomain.BaseDirectory}TuShan.DeleteTaskbarIcon.exe";
            OpenProcessStruct model = null;
            if (model == null)
            {
                model = new OpenProcessStruct
                {
                    Args = "",
                    Path = exePath,
                    WithWindow = false
                };
            }
            WinAPI_Interop.CreateProcess(model.Path, Path.GetDirectoryName(model.Path), model.Args);
        }

        private string GetPathLinkName(string path)
        {
            string[] strings = path.Split('\\');
            string name = strings[strings.Length - 1].Replace(".lnk", "");
            return name;
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
                        if (cleanDeathSetting.Any(c => c.IsEnable && (fileee.Contains(c.AppExeName) || fileee.Contains(c.AppDisplayName) || file.Contains(c.AppDisplayName))))
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


        public static List<string> _NeedDeleteAppFolder = new List<string>();

        private void GetAppCacheFolderPath()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            string localFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string parentDirectory = Directory.GetParent(localFolderPath).FullName;
            string roamingFolderPath = Path.Combine(parentDirectory, "Roaming");
            string localLowFolderPath = Path.Combine(parentDirectory, "LocalLow");
            CleanDeathSetting cleanDeathSetting = ReadCleanDeathSetting();
            if (cleanDeathSetting == null)
            {
                return;
            }
            List<string> exeNameLists = new List<string>();
            foreach (AppSetttingStruct structCleanFolder in cleanDeathSetting.CleanApps)
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


        private void DeleteFolder(string dir, List<string> notDeleteFileList = null, List<string> notDeleteFolderList = null)
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
            CleanDeathSetting cleanDeathSetting = ReadCleanDeathSetting();
            int writeTime = cleanDeathSetting == null ? 3 : cleanDeathSetting.WriteTime;
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
            CleanDeathSetting cleanDeathSetting = ReadCleanDeathSetting();
            if (cleanDeathSetting == null)
            {
                return;
            }
            string keyPath32Bit = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            DeleteAppsFromRegistry(cleanDeathSetting, RegistryView.Registry32, keyPath32Bit);

            DeleteAppsFromRegistry(cleanDeathSetting, RegistryView.Registry64, keyPath32Bit);

            string keyPath64Bit = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
            DeleteAppsFromRegistry(cleanDeathSetting, RegistryView.Registry32, keyPath64Bit);
            DeleteAppsFromRegistry(cleanDeathSetting, RegistryView.Registry64, keyPath64Bit);
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
                            if (!string.IsNullOrWhiteSpace(displayName) && cleanDeathSetting.CleanApps.Any(c => c.AppDisplayName != null && c.IsEnable && displayName.Contains(c.AppDisplayName)))
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
                            if (!string.IsNullOrWhiteSpace(displayName) && cleanDeathSetting.CleanApps.Any(c => c.AppDisplayName != null && c.IsEnable && displayName.Contains(c.AppDisplayName)))
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

        private CleanDeathSetting ReadCleanDeathSetting()
        {
            string parentDirectory = Directory.GetParent(processPath).FullName;
            parentDirectory = Directory.GetParent(parentDirectory).FullName;
            string path = Path.Combine(parentDirectory, "Conf", typeof(CleanDeathSetting).Name + ".json");
            CleanDeathSetting cleanDeathSetting = SettingUtility.GetTSetting<CleanDeathSetting>(true, path);
            return cleanDeathSetting;
        }

        private void SaveCleanDeathSetting(CleanDeathSetting cleanDeathSetting)
        {
            string parentDirectory = Directory.GetParent(processPath).FullName;
            parentDirectory = Directory.GetParent(parentDirectory).FullName;
            string path = Path.Combine(parentDirectory, "Conf", typeof(CleanDeathSetting).Name + ".json");
            SettingUtility.SaveTSetting(cleanDeathSetting, path);
        }

    }
}
