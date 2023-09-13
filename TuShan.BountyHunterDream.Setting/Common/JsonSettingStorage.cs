using System;
using System.IO;
using System.Text;
using TuShan.BountyHunterDream.Logger;

namespace TuShan.BountyHunterDream.Setting.Common
{
    public class JsonSettingStorage<T> : ISettingStorage<T>
    {
        public T setting;
        public T Get(string FileName)
        {
            if (setting != null)
            {
                return setting;
            }
            else
            {
                return Read(FileName);
            }
        }


        public T Read(string fileName)
        {
            if (!File.Exists(fileName))
            {
                TLog.Error("JsonFileNotExist:Path=>" + fileName);
                return default;
            }
            var json = string.Empty;
            try
            {
                using (var fs = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var sr = new StreamReader(fs))
                    {
                        json = sr.ReadToEnd();
                    }
                }
                return JsonUtil.ToObject<T>(json);
            }
            catch (Exception e)
            {
                TLog.Error("(Read)JsonFileError:Path=>" + fileName + "  Error=>" + e.Message);
                TLog.Error(json);
                return default;
            }
        }

        public void Write(T t, string fileName)
        {
            string json = JsonUtil.ToJson<T>(t);
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    fs.Seek(0, SeekOrigin.Begin);
                    fs.Write(bytes, 0, bytes.Length);
                }
            }
            catch (Exception e)
            {
                TLog.Error("(Write)JsonFileError:Path=>" + fileName + "  Error=>" + e.Message);
                TLog.Error(json);
            }
        }
    }
}
