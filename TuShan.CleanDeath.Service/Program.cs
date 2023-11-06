using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using TuShan.BountyHunterDream.Logger;

namespace TuShan.CleanDeath.Service
{
    internal static class Program
    {
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            TLog.Configure($"{AppDomain.CurrentDomain.BaseDirectory}../Conf/Factory/log4net-Service.config");
            ServicesToRun = new ServiceBase[]
            {
                new CleanDeathService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
