using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TuShan.CleanDeath.Service.Struct
{
    public class RequestStruct
    {
        public string ProcessPath { get; set; }
        public SocketEnum SocketEnum { get; set; }
        public Type ArgsType { get; set; }
        public string ArgsJson { get; set; }

        public string USERPROFILEEnvironmentPath { get; set; }
    }

    public class ResponseStruct
    {
        public string ProcessID { get; set; }
        public Type ResultType { get; set; }
        public string ResultsJson { get; set; }
    }

    public class PublicSubStruct
    {
        public SocketEnum SocketEnum { get; set; }
        public string ArgsJson { get; set; }
    }

    public class PublicSubStructParent
    {
        public PublicSubStruct publicSubStruct { get; set; }

        public ExitStruct exitStruct { get; set; }
    }

    public class OpenProcessStruct
    {
        public string Path { get; set; }
        public string Args { get; set; }
        public bool WithWindow { get; set; }
    }

    public class ExitStruct
    {
        public int ProcessID { get; set; }
        public int ExitCode { get; set; }
    }
    public enum SocketEnum
    {
        ConnectServer,
        SendExitCode,
        SetProcessID,
        StartRecoeded,
        Recording,
        StopRecoeded
    }
}
