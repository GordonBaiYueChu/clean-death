using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel.Configuration;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using TuShan.BountyHunterDream.Logger;
using TuShan.CleanDeath.Service.Utility;

namespace TuShan.CleanDeath.Service
{
    partial class CleanDeathService : ServiceBase
    {
        public static string IP = "127.0.0.1";
        public static int Port = 45558;
        public static int PublishPort = 45559;
        public CleanDeathServer _cleanDeathServer;

        public CleanDeathService()
        {
            InitializeComponent();
            _cleanDeathServer = new CleanDeathServer();
            this.CanHandleSessionChangeEvent = true;
        }

        protected override void OnStart(string[] args)
        {
            TLog.Info("CleanDeath Service Start");
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
            _cleanDeathServer?.SessionChanged(changeDescription.Reason);
        }
    }
}
