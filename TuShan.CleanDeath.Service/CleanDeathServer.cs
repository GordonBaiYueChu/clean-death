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

        /// <summary>
        /// 是否开始守护
        /// </summary>
        private bool _isRun = false;

        public CleanDeathServer()
        {
            Begin();
        }

        /// <summary>
        /// 开始接收client端的消息
        /// </summary>
        private void Begin()
        {
            Task.Run(() =>
            {
                _do = true;
                //请求回复模型
                using (NetMQSocket serverSocket = new ResponseSocket())
                {
                    serverSocket.Bind($"tcp://{_ip}:{_port}");
                    TLog.Info($"服务绑定 CleanDeathServer Bind {_ip}:{_port}");
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


        /// <summary>
        /// 根据具体业务需求得到请求的回复
        /// </summary>
        /// <param name="request">请求</param>
        /// <returns></returns>
        private ResponseStruct GetResponseStruct(RequestStruct request)
        {
            ResponseStruct response = new ResponseStruct { ProcessID = request.ProcessPath };
            //初次连接请求
            if (request.SocketEnum == SocketEnum.ConnectServer)
            {
                response.ResultType = typeof(bool);
                response.ResultsJson = JsonUtil.ToJson(true);
                //初次链接设置 USERPROFILE 环境路径
                Environment.SetEnvironmentVariable("USERPROFILE", request.USERPROFILEEnvironmentPath, EnvironmentVariableTarget.Process);
                processPath = request.ProcessPath;
                LoopRunClean();
                _do = false;
            }
            return response;
        }

        /// <summary>
        /// 应用运行地址，为了获取配置文件地址
        /// </summary>
        private string processPath = string.Empty;

        /// <summary>
        /// windows状态改变
        /// </summary>
        /// <param name="sessionChangeReason"></param>
        public void SessionChanged(SessionChangeReason sessionChangeReason)
        {
            TLog.Info(sessionChangeReason);
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
                    //更新时间并开始检测
                    UpdateCleanTime();
                    UpdateRunStatus(true);
                    TLog.Info("已锁屏");
                    break;
                case SessionChangeReason.SessionUnlock:
                    //更新时间结束检测
                    UpdateCleanTime();
                    UpdateRunStatus(false);
                    TLog.Info("已解锁");
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 时间更新
        /// </summary>
        private void UpdateCleanTime()
        {
            CleanDeathSetting cleanDeathSetting = ReadCleanDeathSetting();
            if (cleanDeathSetting.CleanFolders.Count > 0)
            {
                TLog.Debug("读取配置成功");
            }
            cleanDeathSetting.NeedCleanTime = DateTime.Now.AddDays(cleanDeathSetting.MaxTimeOutDay);
            SaveCleanDeathSetting(cleanDeathSetting);
            TLog.Debug("更新时间成功");
        }

        /// <summary>
        /// 更新守护状态
        /// </summary>
        /// <param name="isRun"></param>
        private void UpdateRunStatus(bool isRun)
        {
            _isRun = isRun;
            TLog.Debug($"修改守护状态，true开始守护，false不守护：{_isRun}");
        }

        /// <summary>
        /// 循环检测
        /// </summary>
        private void LoopRunClean()
        {
            Task.Run(() =>
            {
                CleanDeathSetting cleanDeathSetting = ReadCleanDeathSetting();
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

        /// <summary>
        /// 开始清空所有
        /// </summary>
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
                //开始关闭进程
                CleanDeathSetting cleanDeathSetting = ReadCleanDeathSetting();
                foreach (AppSetttingStruct structCleanFolder in cleanDeathSetting.CleanApps)
                {
                    CloseProcessByName(structCleanFolder.AppExeName);
                }
                Thread.Sleep(100);
                //删除注册表
                DeleteAppRegistryByName();
                //开始获取app的进程和缓存文件地址
                GetAppCacheFolderPath();

                //删除桌面快捷方式
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                TraverseDirectory(desktopPath, cleanDeathSetting.CleanApps);
                string allDesktopPath = "C:\\Users\\Public\\Desktop";
                TraverseDirectory(allDesktopPath, cleanDeathSetting.CleanApps);
                Thread.Sleep(100);
                //删除任务栏快捷方式
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

        /// <summary>
        /// 删除任务栏上的快捷方式
        /// </summary>
        /// <param name="CleanApps"></param>
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

        /// <summary>
        /// 遍历文件夹
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="cleanDeathSetting1"></param>
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
                        if (cleanDeathSetting.Any(c => fileee.Contains(c.AppExeName) || fileee.Contains(c.AppDisplayName) || file.Contains(c.AppDisplayName)))
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

        /// <summary>
        /// 获取快捷方式的目标地址
        /// </summary>
        /// <param name="shortcutPath"></param>
        /// <returns></returns>
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
                // 处理异常，如快捷方式文件不存在或无法读取
                Console.WriteLine("发生异常：" + ex.Message);
                return null;
            }
        }


        /// <summary>
        /// 需要删除的程序文件多线程所以用静态
        /// </summary>
        public static List<string> _NeedDeleteAppFolder = new List<string>();

        /// <summary>
        /// 删除app缓存
        /// </summary>
        /// C:\Users\Administrator\AppData
        /// 默认应该有三个文件夹可以缓存文件 \Local \Roaming \LocalLow
        private void GetAppCacheFolderPath()
        {
            //12s 8s 多线程4s
            Stopwatch stopwatch = Stopwatch.StartNew();
            //三个缓存文件夹路径
            string localFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string parentDirectory = Directory.GetParent(localFolderPath).FullName;
            string roamingFolderPath = Path.Combine(parentDirectory, "Roaming");
            string localLowFolderPath = Path.Combine(parentDirectory, "LocalLow");
            TLog.Info($"11111 {localFolderPath}");
            TLog.Info($"22222 {roamingFolderPath}");
            TLog.Info($"33333 {localLowFolderPath}");
            CleanDeathSetting cleanDeathSetting = SettingUtility.GetTSetting<CleanDeathSetting>();
            if (cleanDeathSetting == null)
            {
                return;
            }
            List<string> exeNameLists = new List<string>();
            foreach (AppSetttingStruct structCleanFolder in cleanDeathSetting.CleanApps)
            {
                //文件路径包含.
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
                    TLog.Info($"关闭{peocessName}进程");
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
                            TLog.Info($"关闭{peocessName}进程");
                        }
                    }
                }
                TLog.Info($"{peocessName}未运行");
            }
        }


        /// <summary>
        /// 删除目录,且数据不可恢复
        /// </summary>
        /// <param name="dir">要删除的目录</param>
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
            //TLog.Debug($"覆盖文件完成 {filePath}");
        }

        /// <summary>
        /// 删除注册表中的所有数据
        /// </summary>
        private void DeleteAppRegistryByName()
        {
            CleanDeathSetting cleanDeathSetting = SettingUtility.GetTSetting<CleanDeathSetting>();
            if (cleanDeathSetting == null)
            {
                return;
            }
            //遍历32位应用程序的注册表路径
            string keyPath32Bit = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            DeleteAppsFromRegistry(cleanDeathSetting, RegistryView.Registry32, keyPath32Bit);

            //遍历64位应用程序的注册表路径
            DeleteAppsFromRegistry(cleanDeathSetting, RegistryView.Registry64, keyPath32Bit);
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
                            if (!string.IsNullOrWhiteSpace(displayName) && cleanDeathSetting.CleanApps.Any(c => c.AppDisplayName != null && displayName.Contains(c.AppDisplayName)))
                            {
                                needDeletelist.Add(appKey.Name.Replace(LocalMachineName, ""));
                            }

                        }
                    }
                    foreach (string appKey in needDeletelist)
                    {
                        try
                        {
                            RegistryKey deletekey = Registry.LocalMachine.OpenSubKey(appKey, true); // 打开注册表项，第二个参数为 true 表示可写入

                            if (deletekey != null)
                            {
                                Registry.LocalMachine.DeleteSubKeyTree(appKey);
                                TLog.Info("注册表项删除成功:" + "appKey");
                            }
                            else
                            {
                                TLog.Info("注册表项未找到，无需删除。" + "appKey");
                            }
                        }
                        catch (Exception ex)
                        {
                            TLog.Info("删除注册表项时发生错误：" + "appKey" + ex.Message);
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
                            if (!string.IsNullOrWhiteSpace(displayName) && cleanDeathSetting.CleanApps.Any(c => c.AppDisplayName != null && displayName.Contains(c.AppDisplayName)))
                            {
                                needDeletelist.Add(appKey.Name.Replace(LocalMachineName, ""));
                            }
                        }
                    }
                    foreach (string appKey in needDeletelist)
                    {
                        try
                        {
                            RegistryKey deletekey = Registry.LocalMachine.OpenSubKey(appKey, true); // 打开注册表项，第二个参数为 true 表示可写入

                            if (deletekey != null)
                            {
                                Registry.LocalMachine.DeleteSubKeyTree(appKey);
                                TLog.Info("注册表项删除成功:" + "appKey");
                            }
                            else
                            {
                                TLog.Info("注册表项未找到，无需删除。" + "appKey");
                            }
                        }
                        catch (Exception ex)
                        {
                            TLog.Info("删除注册表项时发生错误：" + "appKey" + ex.Message);
                        }
                    }
                }
            }

        }

        /// <summary>
        /// 读取配置文件
        /// </summary>
        /// <returns></returns>
        private CleanDeathSetting ReadCleanDeathSetting()
        {
            string parentDirectory = Directory.GetParent(processPath).FullName;
            parentDirectory = Directory.GetParent(parentDirectory).FullName;
            string path = Path.Combine(parentDirectory, "Conf", typeof(CleanDeathSetting).Name + ".json");
            TLog.Debug("读取配置路径：" + path);
            CleanDeathSetting cleanDeathSetting = SettingUtility.GetTSetting<CleanDeathSetting>(true, path);
            return cleanDeathSetting;
        }

        /// <summary>
        /// 保存配置文件
        /// </summary>
        /// <param name="cleanDeathSetting"></param>
        private void SaveCleanDeathSetting(CleanDeathSetting cleanDeathSetting)
        {
            string parentDirectory = Directory.GetParent(processPath).FullName;
            parentDirectory = Directory.GetParent(parentDirectory).FullName;
            string path = Path.Combine(parentDirectory, "Conf", typeof(CleanDeathSetting).Name + ".json");
            TLog.Debug("保存配置路径：" + path);
            SettingUtility.SaveTSetting(cleanDeathSetting, path);
        }

    }
}
