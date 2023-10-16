using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReocrdRestartTools
{
    public class RegistryHelp
    {
        /// <summary>
        /// 判断键值是否存在
        /// </summary>
        /// <param name="RegBoot">传入注册表节点</param>
        /// <param name="RegKeyName">节点内字段名称</param>
        /// <returns></returns>
        public static bool IsRegeditKeyExist(RegistryKey RegBoot, string RegKeyName)
        {

            string[] subkeyNames;
            subkeyNames = RegBoot.GetValueNames();
            foreach (string keyName in subkeyNames)
            {

                if (keyName == RegKeyName)  //判断键值的名称
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 更新注册表字段值
        /// </summary>
        /// <param name="rsg">传入注册表节点</param>
        /// <param name="name">节点内字段名称</param>
        /// <param name="value">需要设置的值</param>
        public static void Update(RegistryKey rsg, string name, string value, RegistryValueKind kind)
        {
            if (rsg != null)
            {
                if (!string.IsNullOrEmpty(name))
                {
                    rsg.SetValue(name, value, kind);  //写入
                }
                //rsg.Close();  //关闭
            }
        }

        /// <summary>
        /// 删除注册表字段
        /// </summary>
        /// <param name="rsg">传入注册表节点</param>
        /// <param name="name">节点内字段名称</param>
        public static void Delete(RegistryKey rsg, string name)
        {
            if (rsg != null)
            {
                if (!string.IsNullOrEmpty(name))
                {
                    rsg.DeleteSubKey(name);  //删除
                }
            }
        }

        /// <summary>
        /// 获取注册表节点实体内某个属性的值
        /// </summary>
        /// <param name="key">"SOFTWARE\\Microsoft"格式</param>
        /// <param name="prop">传入属性的键值</param>
        /// <returns></returns>
        public static string ReadPropValue(RegistryKey cameraRoot, string prop)
        {
            var res = "";
            if (cameraRoot != null)
            {
                var obj = cameraRoot.GetValue(prop);
                if (obj != null)
                {
                    res = obj.ToString();
                }
            }
            return res;
        }



        /// <summary>
        /// 判断当前登录用户是否为管理员
        /// </summary>
        /// <returns></returns>
        public static bool IsAdministrator()
        {
            try
            {
                System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
                System.Security.Principal.WindowsPrincipal principal = new System.Security.Principal.WindowsPrincipal(identity);
                //判断当前登录用户是否为管理员
                if (principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator))
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch
            {
                return false;
            }
        }

    }
}
