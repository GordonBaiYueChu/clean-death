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
using TuShan.BountyHunterDream.Service.Utility;

namespace TuShan.BountyHunterDream.Service
{
    public partial class SupervisionService : ServiceBase
    {
        public static string IP = "127.0.0.1";
        public static int Port = 45556;
        public static int PublishPort = 45557;

        private SupervisionServer _supervisionServer;

        public SupervisionService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Port = CommonUtility.GetConfigPort("ResponsePort", true);
            PublishPort = CommonUtility.GetConfigPort("PublishPort", true);
            _supervisionServer = new SupervisionServer(IP, Port, PublishPort);
        }

        protected override void OnStop()
        {
            TLog.Info("TSBHD Service Stop");
        }
    }
}
