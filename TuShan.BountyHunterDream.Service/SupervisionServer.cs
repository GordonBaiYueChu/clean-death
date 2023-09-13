using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using TuShan.BountyHunterDream.Logger;
using TuShan.BountyHunterDream.Service.Struct;
using TuShan.BountyHunterDream.Service.Utility;
using TuShan.BountyHunterDream.Setting.Common;

namespace TuShan.BountyHunterDream.Service
{
    public class SupervisionServer
    {
        private string _ip = "127.0.0.1";
        private int _port = 45556;
        private int _publishPort = 45557;
        private bool _do = false;
        private TimeSpan _ts = new TimeSpan(0, 0, 5);
        private PublisherSocket _pubSocket = new PublisherSocket();

        #region 检测程序状态

        /// <summary>
        /// 监听实时进程状态，状态为无响应就关闭实时进程
        /// </summary>
        private System.Timers.Timer _timer = new System.Timers.Timer();
        private int _maxWaitCount = 10;
        private int _nowWaitCount = 0;
        private bool _isTimerStart = false;
        /// <summary>
        /// 是否监听
        /// </summary>
        private bool _isListenIn = false;

        #endregion

        /// <summary>
        /// 守护的进程id列表
        /// </summary>
        public List<string> _processList = new List<string>();

        public SupervisionServer(string ip, int port, int publishPort)
        {
            _ip = ip;
            _port = port;
            _publishPort = publishPort;
            Begin();
        }

        private void Begin()
        {
            Task.Run(() =>
            {
                _do = true;
                //请求回复模型
                using (NetMQSocket serverSocket = new ResponseSocket())
                {
                    serverSocket.Bind($"tcp://{_ip}:{_port}");
                    TLog.Info($"SupervisionService Bind {_ip}:{_port}");
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
                                TLog.Debug($"server ReceiveFrame {requestStr}");
                                var request = JsonUtil.ToObject<RequestStruct>(requestStr);
                                ResponseStruct response = GetResponseStruct(request);
                                var responseStr = JsonUtil.ToJson(response);
                                serverSocket.TrySendFrame(responseStr);
                                TLog.Debug($"server SendFrame {responseStr}");
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
            TLog.Info($"GetResponseStruct() {request.ProcessID}, {request.SocketEnum}, {request.ArgsType}, {request.ArgsJson}");

            ResponseStruct response = new ResponseStruct { ProcessID = request.ProcessID };
            //初次连接请求
            if (request.SocketEnum == SocketEnum.ConnectServer)
            {
                UpdateProcess(request);
                response.ResultType = typeof(bool);
                response.ResultsJson = JsonUtil.ToJson(true);
                //初次链接设置 USERPROFILE 环境路径
                Environment.SetEnvironmentVariable("USERPROFILE", request.USERPROFILEEnvironmentPath, EnvironmentVariableTarget.Process);
            }
            //开始计时心跳
            else if (request.SocketEnum == SocketEnum.StartRecoeded)
            {
                response.ResultType = typeof(bool);
                response.ResultsJson = JsonUtil.ToJson(true);
                StartRecordTime();
                _isListenIn = true;
                TLog.Info($"Server : Received Start Recording Message");
            }
            //刷新心跳计时
            else if (request.SocketEnum == SocketEnum.Recording)
            {
                response.ResultType = typeof(bool);
                response.ResultsJson = JsonUtil.ToJson(true);
                RefreshWaitTime();
            }
            // 停止心跳计时，重置参数
            else if (request.SocketEnum == SocketEnum.StopRecoeded)
            {
                response.ResultType = typeof(bool);
                response.ResultsJson = JsonUtil.ToJson(true);
                StopRecordTime();
                _nowWaitCount = 0;
                _isListenIn = false;
                TLog.Info($"Server : Received Stop Recording Message");
            }
            return response;
        }

        private static object _lockObject = new object();

        /// <summary>
        /// 收到客户端连接后,更新客户端程序的缓存并注册关闭事件
        /// </summary>
        /// <param name="request"></param>
        private void UpdateProcess(RequestStruct request)
        {
            TLog.Error(request.ProcessID);
            // lock (_lockObject)
            {
                if (!_processList.Contains(request.ProcessID))
                {

                    _processList.Add(request.ProcessID);

                    Process process = Process.GetProcessById(Convert.ToInt32(request.ProcessID));
                    process.EnableRaisingEvents = true;
                    process.Exited += (sender, e) =>
                    {

                        ProcessStateCodeEnum state = ProcessStateCodeEnum.ExitCodeSuccess;
                        TLog.Error($"Process with ID {request.ProcessID} has exited.{(int)state}");
                        try
                        {
                            state = (ProcessStateCodeEnum)process.ExitCode;
                        }
                        catch (Exception ex)
                        {
                            state = ProcessStateCodeEnum.ExitCodeUnKnown;
                        }

                        _processList?.Remove(request.ProcessID);

                        //如果是异常关闭,重启
                        if (state != ProcessStateCodeEnum.ExitCodeSuccess
                        && (int)state != -1)
                        {
                            TLog.Error($"Server : Main Process Restart,state:{state}");
                            ProcessUtility.OpenMainProcess();
                        }

                        TLog.Error($"Process with ID {request.ProcessID} has exited.{(int)state}");
                    };
                }
            }

        }


        #region 进程状态监听

        /// <summary>
        /// 显示录制时间
        /// </summary>
        private void StartRecordTime()
        {
            if (_isTimerStart)
            {
                _nowWaitCount++;
                return;
            }
            _timer.Interval = 1000 * 60;
            _timer.Elapsed += new ElapsedEventHandler(HeardTime_Tick);
            _timer.Enabled = true;
            _timer.Start();
            _nowWaitCount = 0;
            _isTimerStart = true;
        }

        /// <summary>
        /// timer方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HeardTime_Tick(object sender, ElapsedEventArgs e)
        {
            TLog.Info("Server : NowWaitCount" + _nowWaitCount.ToString());
            ///process
            foreach (string id in _processList)
            {

                _nowWaitCount++;
                int processId = Convert.ToInt32(id);
                if (_nowWaitCount >= _maxWaitCount)
                {
                    StopRecordTime();
                    ProcessUtility.KillProcess(processId);
                    TLog.Info("Server : Kill Monitor Because Monitor Is No Response");
                }

            }
        }

        /// <summary>
        /// 重置等待时间
        /// </summary>
        private void RefreshWaitTime()
        {
            _nowWaitCount = 0;
        }

        /// <summary>
        /// 停止计时
        /// </summary>
        private void StopRecordTime()
        {
            if (!_isTimerStart)
            {
                return;
            }
            _timer.Elapsed -= new ElapsedEventHandler(HeardTime_Tick);
            _timer.Stop();
            _isTimerStart = false;
            _nowWaitCount = 0;
        }

        #endregion
    }
}
