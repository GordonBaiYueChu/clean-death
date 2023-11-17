using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using TuShan.BountyHunterDream.Logger;
using TuShan.BountyHunterDream.Setting.Common;

namespace TuShan.BountyHunterDream.Setting
{
    public class SettingUtility
    {
        public static string basePath = "../Conf/";
        private const string ExName = ".json";
        private const string BackExName = ".bak";
        private static Dictionary<string, object> _cacheSetting = new Dictionary<string, object>();
        private static string _lastMessage = "Language_FileDestroyedNeedUseLast";
        private static string _factoryMessage = "Language_FileDestroyedNeedUseFactory";
        public static string factoryPath = "../Conf/Factory/";
        private const string FacExName = ".bak";

        /// <summary>
        /// 不比较的属性名称
        /// </summary>
        private static List<string> _notCompareTypeName = new List<string>() { "Guid", "Version" };


        public static void CheckFilesExist()
        {
            CheckFileExist("CleanDeathSetting");
        }

        public static void CheckFileExist(string filename)
        {
            string filePath = string.Format("../Conf/{0}.json", filename);
            string lastFile = string.Format("../Conf/{0}.bak", filename);
            string factoryFile = string.Format("../Conf/Factory/{0}.bak", filename);
            if (!File.Exists(filePath))
            {
                if (!File.Exists(lastFile))
                {
                    if (File.Exists(factoryFile))
                    {
                        File.Copy(factoryFile, filePath);
                    }
                }
                else
                {
                    File.Copy(lastFile, filePath);
                }
            }
        }

        #region save

        /// <summary>
        /// 保存配置文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// 不需要替换对象，直接保存文件即可
        /// 1.当导入时，需要保存导入的值到文件中，并拷贝要导入的值放进内存中。
        public static void SaveTSetting<T>(T t, bool isUpdateCache = false) where T : BaseSetting<T>, new()
        {
            bool isExist = true;
            string jsonName = t.GetType().Name;
            string confFileName = basePath + jsonName + ExName;
            string confBackFileName = basePath + jsonName + BackExName;
            if (!_cacheSetting.ContainsKey(jsonName))
            {
                isExist = false;
                _cacheSetting.Add(jsonName, t);
            }
            T cache = _cacheSetting[jsonName] as T;
            SaveCache<T>(t, ref cache, confFileName, confBackFileName);
            if (isUpdateCache && isExist && t != null)
            {
                AddUpdateCache(t, jsonName);
            }
        }


        /// <summary>
        /// 保存配置文件带路径
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// 不需要替换对象，直接保存文件即可
        /// 1.当导入时，需要保存导入的值到文件中，并拷贝要导入的值放进内存中。
        public static void SaveTSetting<T>(T t, string path, bool isUpdateCache = false) where T : BaseSetting<T>, new()
        {
            bool isExist = true;
            string jsonName = t.GetType().Name;
            if (!_cacheSetting.ContainsKey(jsonName))
            {
                isExist = false;
                _cacheSetting.Add(jsonName, t);
            }
            T cache = _cacheSetting[jsonName] as T;
            SaveCache<T>(t, ref cache, path, path);
            if (isUpdateCache && isExist && t != null)
            {
                AddUpdateCache(t, jsonName);
            }
        }


        /// <summary>
        /// 保存配置对象，对象为空则保存原来的对象。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toSave"></param>
        /// <param name="cache"></param>
        /// <param name="path"></param>
        /// <param name="lastpath"></param>
        /// <param name="checkNull"></param>
        private static void SaveCache<T>(T toSave, ref T cache, string path, string lastpath, Func<T, bool> checkNull = null) where T : BaseSetting<T>, new()
        {
            if (checkNull != null && checkNull(toSave) || toSave == null)
            {
                if (checkNull != null && checkNull(cache) || cache == null)
                    return;
                toSave = cache;
            }

            SaveToJson(toSave, path, lastpath);
        }

        private static void SaveToJson<T>(T t, string path, string lastpath) where T : BaseSetting<T>, new()
        {
            t.Write(t, path);
        }
        #endregion

        #region load
        public static T GetTSetting<T>(bool reload = false, string path = "", bool isTipBreakDownConfigFile = true) where T : BaseSetting<T>, new()
        {
            string jsonName = typeof(T).Name;
            if (isTipBreakDownConfigFile)
                return GetTSetting<T>(FuncJsonMessageShow(jsonName + ExName, _lastMessage), FuncJsonMessageShow(jsonName + BackExName, _factoryMessage), reload, path);
            else
                return GetTSetting<T>(null, null, reload, path);
        }

        private static T GetTSetting<T>(Func<bool> uselast = null, Func<bool> usefactory = null, bool reload = false, string path = "") where T : BaseSetting<T>, new()
        {
            string fileName = typeof(T).Name;
            if (string.IsNullOrEmpty(path))
            {
                string confFileName = basePath + fileName + ExName;
                string confBackFileName = basePath + fileName + BackExName;
                string confFacFileName = factoryPath + fileName + FacExName;

                return GetCacheTFromJson<T>(reload, fileName, confFileName, confBackFileName, confFacFileName, uselast, usefactory);
            }
            else
            {

                return GetCacheTFromJson<T>(reload, fileName, path, path, path, uselast, usefactory);

            }
        }

        private static T GetCacheTFromJson<T>(bool reload, string fileName, string path, string lastpath, string factorypath, Func<bool> uselast = null, Func<bool> usefactory = null) where T : BaseSetting<T>, new()
        {
            T cache = null;
            if (reload || !_cacheSetting.ContainsKey(fileName) || _cacheSetting[fileName] == null)
            {
                cache = LoadTFromJson<T>(path);
                AddUpdateCache(cache, fileName);
            }
            else
            {
                cache = (T)_cacheSetting[fileName];
            }

            if (!_cacheSetting.ContainsKey(fileName) || _cacheSetting[fileName] == null || cache.CheckNull(cache))
            {
                if (uselast == null || uselast.Invoke())
                {
                    cache = LoadTFromJson<T>(lastpath);
                    AddUpdateCache(cache, fileName);
                    if (!_cacheSetting.ContainsKey(fileName) || _cacheSetting[fileName] == null || cache.CheckNull(cache))
                    {
                        if (usefactory == null || usefactory.Invoke())
                        {
                            cache = LoadTFromJson<T>(factorypath);
                            AddUpdateCache(cache, fileName);
                        }
                    }
                    cache.Write(cache, path);
                }
            }

            return (T)_cacheSetting[fileName];
        }

        /// <summary>
        /// 更新配置文件对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <param name="key"></param>
        /// 直接替换值会发生内存泄漏
        private static void AddUpdateCache<T>(T t, string key) where T : BaseSetting<T>
        {
            if (_cacheSetting.ContainsKey(key))
            {
                TLog.Info($"保存配置文件{typeof(T).Name}");
                //相同对象不进行保存处理
                if (_cacheSetting[key] == t)
                {
                    return;
                }
                SetPropertysEquale(_cacheSetting[key], t, _notCompareTypeName);
            }
            else
            {
                _cacheSetting.Add(key, t);
            }
        }

        /// <summary>
        /// 修改类的属性值(不考虑顺序)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="soureData">元数据</param>
        /// <param name="needCopyData">待复制数据</param>
        /// <param name="notCompareTypeName">不比较的属性名称</param>
        /// <returns></returns>
        public static void SetPropertysEquale<T>(T soureData, T needCopyData, List<string> notCompareTypeName = null)
        {
            if (soureData == null || needCopyData == null)
            {
                TLog.Info($"设置配置文件{typeof(T).Name}失败，无数据");
                return;
            }
            Type sInfo = soureData.GetType();
            if (sInfo.IsGenericType)
            {
                if (soureData is System.Collections.IList sourelist && needCopyData is System.Collections.IList needCopylist)
                {
                    if (sourelist.Count < 1)
                    {
                        return;
                    }
                    sourelist.Clear();
                    for (int i = 0; i < needCopylist.Count; i++)
                    {
                        sourelist.Add(needCopylist[i]);
                    }
                }
                return;
            }
            else
            {
                PropertyInfo[] sInfos = sInfo.GetProperties();
                foreach (PropertyInfo sourePro in sInfos)
                {
                    //属性名
                    string Pname = sourePro.Name;
                    //属性类型
                    string pTypeName = sourePro.PropertyType.Name;
                    //值
                    object sourevalue = sourePro.GetValue(soureData, null);
                    object needCopyValue = sourePro.GetValue(needCopyData, null);
                    if (sourePro.PropertyType.IsValueType || pTypeName.Equals("String") || pTypeName.Equals("SolidColorBrush"))
                    {
                        string soureValue = sourevalue == null ? string.Empty : sourevalue.ToString();
                        string comparativeValue = needCopyValue == null ? string.Empty : needCopyValue.ToString();
                        if (!soureValue.Equals(comparativeValue) && (notCompareTypeName == null || (notCompareTypeName != null && !notCompareTypeName.Contains(Pname))))
                        {
                            sourePro.SetValue(soureData, Convert.ChangeType(needCopyValue, sourePro.PropertyType), null);
                        }
                    }
                    else if (sourevalue is System.Collections.IList sourelist && needCopyValue is System.Collections.IList needCopylist)
                    {
                        //为数组结构，数量未知，对象赋值
                        if (sourelist.Count > 0 && (sourelist[0].GetType().IsValueType || sourelist[0].GetType().Name.StartsWith("String")))
                        {
                            sourePro.SetValue(soureData, Convert.ChangeType(needCopyValue, sourePro.PropertyType), null);
                        }
                        else if (sourelist.Count > 0 && !sourelist[0].GetType().IsValueType)
                        {
                            SetPropertysEquale(sourevalue, needCopyValue, notCompareTypeName);
                        }
                    }
                    else
                    {
                        SetPropertysEquale(sourevalue, needCopyValue, notCompareTypeName);
                    }
                }
            }
        }

        public static T LoadTFromJson<T>(string path) where T : BaseSetting<T>, new()
        {
            T t = new T();
            t = t.Read(path);
            return t;
        }

        private static Func<bool> FuncJsonMessageShow(string jsonName, string message)
        {
            return () =>
            {
                bool isOk = false;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (Application.Current.MainWindow != null)
                    {
                        //var MessageBoxResult = TMessageBox.Show(jsonName + " " + LanguageHelper.LoadString(message), ButtonType.OK, MessageType.Warn, true, 3);
                        //isOk = MessageBoxResult == MessageBoxResult.OK || MessageBoxResult == MessageBoxResult.Yes;
                    }
                    else
                    {
                        isOk = true;
                    }
                });
                return isOk;
            };
        }
        #endregion
    }
}
