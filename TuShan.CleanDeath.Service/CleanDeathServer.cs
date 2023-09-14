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

namespace TuShan.CleanDeath.Service
{
    public class CleanDeathServer
    {
        private string _ip = "127.0.0.1";
        private int _port = 45556;
        private int _publishPort = 45557;

        public CleanDeathServer(string ip, int port, int publishPort)
        {
            _ip = ip;
            _port = port;
            _publishPort = publishPort;
            Begin();
        }

        public void Begin()
        {
            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
            TLog.Info("等待系统事件...");
        }

        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (e.Reason == SessionSwitchReason.SessionLock)
            {
                TLog.Info("系统已锁屏.");
            }
            else if (e.Reason == SessionSwitchReason.SessionUnlock)
            {
                TLog.Info("系统已解锁.");
            }
        }
    }
}
