using NetMQ.Sockets;
using NetMQ;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using TuShan.BountyHunterDream.Logger;
using TuShan.BountyHunterDream.Setting.Common;
using TuShan.CleanDeath.Service.Struct;
using TuShan.CleanDeath.Service.Utility;
using Microsoft.Win32;
using System.ServiceProcess;
using TuShan.BountyHunterDream.Setting.Setting;
using TuShan.BountyHunterDream.Setting;
using System.Threading;
using TuShan.BountyHunterDream.Setting.Struct;
using System.IO;
using System.Security.Cryptography;

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
                TLog.Debug($"接收到程序运行路径processPath: {processPath}");
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
                        _isRun = false;
                        Thread.Sleep(1000);
                    }
                }
            });
        }

        /// <summary>
        /// 开始清空所有
        /// </summary>
        private void StartClean()
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
            string path = Path.Combine(parentDirectory, "Config", typeof(CleanDeathSetting).Name + ".json");
            TLog.Debug("保存配置路径：" + path);
            SettingUtility.SaveTSetting(cleanDeathSetting, path);
        }

    }
}
