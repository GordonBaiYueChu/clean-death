using System;
using System.Configuration;
using System.Linq;
using System.Net.NetworkInformation;

namespace TuShan.CleanDeath.Service.Utility
{
    public class CommonUtility
    {
        /// <summary>
        /// 检测端口是否占用并返回可用端口
        /// </summary>
        /// <returns></returns>
        private static int GetFreePort(int port)
        {
            var random = new Random();
            while (IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners().Any(p => p.Port == port))
            {
                port = random.Next(10000, 65535);
            }
            return port;
        }

        /// <summary>
        /// 读取指定key的端口号
        /// 从配置文件拿取port
        /// 如果port已经占用，随机取空闲port，并写入配置文件
        /// </summary>
        /// <param name="portKey">端口key</param>
        /// <param name="isServer">是否是服务端，如果是，需要判断端口是否占用，如果占用就取空闲端口并修改保存配置文件</param>
        /// <returns></returns>
        public static int GetConfigPort(string portKey, bool isServer)
        {
            ExeConfigurationFileMap filemap = new ExeConfigurationFileMap();
            filemap.ExeConfigFilename = $"{AppDomain.CurrentDomain.BaseDirectory}TuShan.BountyHunterDream.Service.exe.config";//配置文件路径
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(filemap, ConfigurationUserLevel.None);
            if (!AppSettingsKeyExists(portKey, config))
            {
                config.AppSettings.Settings[portKey].Value = GetFreePort(45557).ToString();
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

        /// <summary>
        /// 判断appSettings中是否有此项
        /// </summary>
        /// <param name="strKey"></param>
        /// <param name="config"></param>
        /// <returns></returns>
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
