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
        public static bool IsRegeditKeyExist(RegistryKey RegBoot, string RegKeyName)
        {

            string[] subkeyNames;
            subkeyNames = RegBoot.GetValueNames();
            foreach (string keyName in subkeyNames)
            {

                if (keyName == RegKeyName)  
                {
                    return true;
                }
            }
            return false;
        }

        public static void Update(RegistryKey rsg, string name, string value, RegistryValueKind kind)
        {
            if (rsg != null)
            {
                if (!string.IsNullOrEmpty(name))
                {
                    rsg.SetValue(name, value, kind);  
                }
            }
        }

        public static void Delete(RegistryKey rsg, string name)
        {
            if (rsg != null)
            {
                if (!string.IsNullOrEmpty(name))
                {
                    rsg.DeleteSubKey(name);  
                }
            }
        }

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



        public static bool IsAdministrator()
        {
            try
            {
                System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
                System.Security.Principal.WindowsPrincipal principal = new System.Security.Principal.WindowsPrincipal(identity);
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
