using System;
using System.Configuration;
using System.Linq;
using System.Net.NetworkInformation;

namespace TuShan.CleanDeath.Service.Utility
{
    public class CommonUtility
    {
        private static int GetFreePort(int port)
        {
            var random = new Random();
            while (IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners().Any(p => p.Port == port))
            {
                port = random.Next(10000, 65535);
            }
            return port;
        }

        public static int GetConfigPort(string portKey, bool isServer)
        {
            ExeConfigurationFileMap filemap = new ExeConfigurationFileMap();
            filemap.ExeConfigFilename = $"{AppDomain.CurrentDomain.BaseDirectory}TuShan.CleanDeath.Service.exe.config";
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(filemap, ConfigurationUserLevel.None);
            if (!AppSettingsKeyExists(portKey, config))
            {
                config.AppSettings.Settings[portKey].Value = GetFreePort(45559).ToString();
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
            else
            {
                int port = Convert.ToInt32(config.AppSettings.Settings[portKey].Value);
                if (isServer)
                {
                    port = GetFreePort(port);
                    if (port.ToString() != config.AppSettings.Settings[portKey].Value)
                    {
                        config.AppSettings.Settings[portKey].Value = port.ToString();
                        config.Save(ConfigurationSaveMode.Modified);
                        ConfigurationManager.RefreshSection("appSettings");
                    }
                }
            }
            return Convert.ToInt32(config.AppSettings.Settings[portKey].Value);
        }

        private static bool AppSettingsKeyExists(string strKey, Configuration config)
        {
            foreach (string str in config.AppSettings.Settings.AllKeys)
            {
                if (str == strKey)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
