using System.ServiceProcess;
using TuShan.BountyHunterDream.Logger;

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

        protected override void OnSessionChange(SessionChangeDescription changeDescription)
        {
            _cleanDeathServer?.SessionChanged(changeDescription.Reason);
        }
    }
}
