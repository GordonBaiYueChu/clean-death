using Microsoft.Win32;
using NetMQ.Sockets;
using NetMQ;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TuShan.BountyHunterDream.Logger;
using TuShan.BountyHunterDream.Setting.Common;
using TuShan.CleanDeath.Service.Struct;
using TuShan.CleanDeath.Service.Utility;

namespace TuShan.CleanDeath.Helps
{
    /// <summary>
    /// 客户端
    /// </summary>
    public class ServiceClient
    {
        private string _processPath;  //进程ID,身份唯一标识
        private string _ip; //服务端ip
        private int _port;  //服务端端口
        private int _publishPort;   //推送接收方式服务端端口
        private TimeSpan _ts = new TimeSpan(0, 0, 5);

        public ServiceClient(string processPath, string ip, int port, int publishPort)
        {
            _processPath = processPath;
            _ip = ip;
            _port = port;
            _publishPort = publishPort;
            TLog.Debug($"Client Init ip:{ip} port:{port} publishPort:{publishPort}");
            Begin();
            ConnectServer();
        }

        /// <summary>
        /// 监听server发送的消息
        /// </summary>
        private void Begin()
        {
            Task.Run(() =>
            {
                //请求回复模型
                using (SubscriberSocket subSocket = new SubscriberSocket())
                {
                    subSocket.Options.ReceiveHighWatermark = 1000;
                    subSocket.Connect($"tcp://{_ip}:{_publishPort}");
                    subSocket.Subscribe(_processPath);
                    TLog.Debug($"CleanDeathService Bind {_ip}:{_publishPort}");
                    while (true)
                    {
                        try
                        {
                            string messageTopicReceived = "";
                            bool retTopic = subSocket.TryReceiveFrameString(_ts, out messageTopicReceived);
                            if (retTopic)
                            {
                                string messageReceived = "";
                                bool ret = subSocket.TryReceiveFrameString(_ts, out messageReceived);
                                if (ret)
                                {
                                    var publicSub = JsonUtil.ToObject<PublicSubStruct>(messageReceived);
                                    Task.Run(() =>
                                    {
                                        HandlePublicSub(publicSub);
                                    });
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            TLog.Error(ex.ToString());
                        }
                    }
                }
            });
        }

        /// <summary>
        /// 处理服务端发送的消息
        /// </summary>
        /// <param name="publicSubStruct"></param>
        private void HandlePublicSub(PublicSubStruct publicSubStruct)
        {

        }

        /// <summary>
        /// 连接到Server,本进程ID,作为身份唯一标识传递
        /// </summary>
        /// <returns></returns>
        public bool ConnectServer()
        {
            string path = Environment.GetEnvironmentVariable("USERPROFILE", EnvironmentVariableTarget.Process);
            var request = new RequestStruct { ProcessPath = _processPath, SocketEnum = SocketEnum.ConnectServer, ArgsType = typeof(string), ArgsJson = JsonUtil.ToJson(""), USERPROFILEEnvironmentPath = path };
            var ret = Request<bool>(request);
            return ret;
        }

        private T Request<T>(RequestStruct request)
        {
            ResponseStruct ret = Request(request);
            if (ret == null)
            {
                TLog.Debug($"Request Service TimeOut!");
                return default;
            }
            else
            {
                return JsonUtil.ToObject<T>(ret.ResultsJson);
            }
        }


        /// <summary>
        /// Request请求
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private ResponseStruct Request(RequestStruct request)
        {
            using (NetMQSocket clientSocket = new RequestSocket())
            {
                clientSocket.Connect($"tcp://{_ip}:{_port}");
                var requestStr = JsonUtil.ToJson(request);
                clientSocket.SendFrame(requestStr);
                string responseStr = "";
                bool ret = clientSocket.TryReceiveFrameString(_ts, out responseStr);
                if (ret)
                {
                    return JsonUtil.ToObject<ResponseStruct>(responseStr);
                }
                else
                {
                    return null;
                }
            }
        }

    }

    public class ServiceClientUtility
    {
        public static ServiceClient Client;

        /// <summary>
        /// 初始化服务客户端
        /// </summary>
        public static void InitClient()
        {
            string serverIp = "127.0.0.1";
            Client = new ServiceClient(AppDomain.CurrentDomain.BaseDirectory, serverIp, CommonUtility.GetConfigPort("ResponsePort", false), CommonUtility.GetConfigPort("PublishPort", false));
        }
    }

    public class ServiceUtility
    {
        private static string _serviceName = "CleanDeathService";

        public static void StartService()
        {
            bool ret = false;
            if (ServiceAPI.isServiceIsExisted(_serviceName))
            {
                string path = ServiceAPI.GetWindowsServiceInstallPath(_serviceName);
                string pathNow = AppDomain.CurrentDomain.BaseDirectory;
                ret = string.Compare(Path.GetFullPath(path).TrimEnd('\\'), Path.GetFullPath(pathNow).TrimEnd('\\'), StringComparison.InvariantCultureIgnoreCase) != 0;
            }
            if (ServiceAPI.GetServiceStatus(_serviceName) != ServiceControllerStatus.Running || ret)
            {
                Process proc = new Process();
                proc.StartInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                proc.StartInfo.FileName = "StartService.bat";
                proc.StartInfo.CreateNoWindow = true;
                proc.Start();
                proc.WaitForExit();
                Thread.Sleep(2000);
                //等待服务运行
                while (ServiceAPI.GetServiceStatus(_serviceName) != ServiceControllerStatus.Running)
                {

                }
            }
        }

        public static void StopService()
        {
            if (ServiceAPI.isServiceIsExisted(_serviceName))
            {
                ServiceControllerStatus status = ServiceAPI.GetServiceStatus(_serviceName);
                if (status == ServiceControllerStatus.Running)
                {
                    Process proc = new Process();
                    proc.StartInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    proc.StartInfo.FileName = "StopService.bat";
                    proc.StartInfo.CreateNoWindow = true;
                    proc.Start();
                    proc.WaitForExit();
                }
            }
        }

        /// <summary>
        /// 检测端口是否占用并返回可用端口
        /// </summary>
        /// <returns></returns>
        public static int GetFreePort(int port)
        {
            var random = new Random();
            while (IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners().Any(p => p.Port == port) || port < 10000 || port > 65535)
            {
                port = random.Next(10000, 65535);
            }
            return port;
        }
    }

    public class ServiceAPI
    {
        /// <summary>
        /// 检查服务存在的存在性
        /// </summary>
        /// <param name=" NameService ">服务名</param>
        /// <returns>存在返回 true,否则返回 false;</returns>
        public static bool isServiceIsExisted(string NameService)
        {
            ServiceController[] services = ServiceController.GetServices();
            foreach (ServiceController s in services)
            {
                if (s.ServiceName.ToLower() == NameService.ToLower())
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 安装Windows服务
        /// </summary>
        /// <param name="stateSaver">集合</param>
        /// <param name="filepath">程序文件路径</param>
        public static void InstallmyService(IDictionary stateSaver, string filepath)
        {
            AssemblyInstaller AssemblyInstaller1 = new AssemblyInstaller();
            AssemblyInstaller1.UseNewContext = true;
            AssemblyInstaller1.Path = filepath;
            AssemblyInstaller1.Install(stateSaver);
            AssemblyInstaller1.Commit(stateSaver);
            AssemblyInstaller1.Dispose();
        }
        /// <summary>
        /// 卸载Windows服务
        /// </summary>
        /// <param name="filepath">程序文件路径</param>
        public static void UnInstallmyService(string filepath)
        {
            AssemblyInstaller AssemblyInstaller1 = new AssemblyInstaller();
            AssemblyInstaller1.UseNewContext = true;
            AssemblyInstaller1.Path = filepath;
            AssemblyInstaller1.Uninstall(null);
            AssemblyInstaller1.Dispose();
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        /// <param name=" NameService ">服务名</param>
        /// <returns>存在返回 true,否则返回 false;</returns>
        public static bool RunService(string NameService)
        {
            bool bo = true;
            try
            {
                ServiceController sc = new ServiceController(NameService);
                if (sc.Status.Equals(ServiceControllerStatus.Stopped) || sc.Status.Equals(ServiceControllerStatus.StopPending))
                {
                    sc.Start();
                }
            }
            catch
            {
                bo = false;
            }

            return bo;
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        /// <param name=" NameService ">服务名</param>
        /// <returns>存在返回 true,否则返回 false;</returns>
        public static bool StopService(string NameService)
        {
            bool bo = true;
            try
            {
                ServiceController sc = new ServiceController(NameService);
                if (!sc.Status.Equals(ServiceControllerStatus.Stopped))
                {
                    sc.Stop();
                }
            }
            catch
            {
                bo = false;
            }

            return bo;
        }

        /// <summary>
        /// 获取服务状态
        /// </summary>
        /// <param name=" NameService ">服务名</param>
        /// <returns>返回服务状态</returns>
        public static ServiceControllerStatus GetServiceStatus(string NameService)
        {
            try
            {
                if (isServiceIsExisted(NameService))
                {
                    ServiceController sc = new ServiceController(NameService);
                    return sc.Status;
                }
                else
                {
                    return ServiceControllerStatus.Stopped;
                }
            }
            catch
            {
                return ServiceControllerStatus.Stopped;
            }
        }

        /// <summary>
        /// 获取服务安装路径
        /// </summary>
        /// <param name="ServiceName"></param>
        /// <returns></returns>
        public static string GetWindowsServiceInstallPath(string ServiceName)
        {
            string path = "";
            try
            {
                string key = @"SYSTEM\CurrentControlSet\Services\" + ServiceName;
                path = Registry.LocalMachine.OpenSubKey(key).GetValue("ImagePath").ToString();

                path = path.Replace("\"", string.Empty);//替换掉双引号  

                FileInfo fi = new FileInfo(path);
                path = fi.Directory.ToString();
            }
            catch
            {
                path = "";
            }
            return path;
        }

        /// <summary>
        /// 获取指定服务的版本号
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <returns></returns>
        public static string GetServiceVersion(string serviceName)
        {
            if (string.IsNullOrEmpty(serviceName))
            {
                return string.Empty;
            }
            try
            {
                string path = GetWindowsServiceInstallPath(serviceName) + "\\" + serviceName + ".exe";
                Assembly assembly = Assembly.LoadFile(path);
                AssemblyName assemblyName = assembly.GetName();
                Version version = assemblyName.Version;
                return version.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
