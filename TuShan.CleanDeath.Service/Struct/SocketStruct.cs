using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TuShan.CleanDeath.Service.Struct
{
    /// <summary>
    /// 请求
    /// </summary>
    public class RequestStruct
    {
        public string ProcessID { get; set; }
        public SocketEnum SocketEnum { get; set; }
        public Type ArgsType { get; set; }
        public string ArgsJson { get; set; }

        /// <summary>
        /// USERPROFILE环境变量的值,在service打开进程
        /// </summary>
        public string USERPROFILEEnvironmentPath { get; set; }
    }

    /// <summary>
    /// 回复
    /// </summary>
    public class ResponseStruct
    {
        public string ProcessID { get; set; }
        public Type ResultType { get; set; }
        public string ResultsJson { get; set; }
    }

    /// <summary>
    /// 服务端推送
    /// </summary>
    public class PublicSubStruct
    {
        public SocketEnum SocketEnum { get; set; }
        public string ArgsJson { get; set; }
    }

    /// <summary>
    /// 服务端推送备份数据
    /// </summary>
    public class PublicSubStructParent
    {
        public PublicSubStruct publicSubStruct { get; set; }

        public ExitStruct exitStruct { get; set; }
    }

    /// <summary>
    /// 打开进程
    /// </summary>
    public class OpenProcessStruct
    {
        /// <summary>
        /// exe路径
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// 启动参数
        /// </summary>
        public string Args { get; set; }
        /// <summary>
        /// 是否带窗口
        /// </summary>
        public bool WithWindow { get; set; }
    }

    /// <summary>
    /// 子进程退出
    /// </summary>
    public class ExitStruct
    {
        /// <summary>
        /// 子进程ID
        /// </summary>
        public int ProcessID { get; set; }
        /// <summary>
        /// 子进程退出值
        /// </summary>
        public int ExitCode { get; set; }
    }
    /// <summary>
    /// Socket通信参数枚举类
    /// 用于区分参数方法
    /// </summary>
    public enum SocketEnum
    {
        /// <summary>
        /// 连接
        /// </summary>
        ConnectServer,
        /// <summary>
        /// 传递子进程退出返回值
        /// </summary>
        SendExitCode,
        /// <summary>
        /// 通知后台进程ID
        /// </summary>
        SetProcessID,
        /// <summary>
        /// 开始录制
        /// </summary>
        StartRecoeded,
        /// <summary>
        /// 录制中
        /// </summary>
        Recording,
        /// <summary>
        /// 停止录制
        /// </summary>
        StopRecoeded
    }
}
