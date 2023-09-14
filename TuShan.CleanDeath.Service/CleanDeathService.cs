using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using TuShan.BountyHunterDream.Logger;
using TuShan.CleanDeath.Service.Utility;

namespace TuShan.CleanDeath.Service
{
    partial class CleanDeathService : ServiceBase
    {

        public CleanDeathService()
        {
            InitializeComponent();
            this.CanHandleSessionChangeEvent = true;
        }

        protected override void OnStart(string[] args)
        {
        }

        protected override void OnStop()
        {
            TLog.Info("CleanDeath Service Stop");
        }

        /// <summary>
        /// 监听windows信息
        /// </summary>
        /// <param name="changeDescription"></param>
        protected override void OnSessionChange(SessionChangeDescription changeDescription)
        {
            switch (changeDescription.Reason)
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
                    TLog.Info("已锁屏");
                    break;
                case SessionChangeReason.SessionUnlock:
                    TLog.Info("已解锁");
                    break;
                default:
                    break;
            }
        }
    }
}
