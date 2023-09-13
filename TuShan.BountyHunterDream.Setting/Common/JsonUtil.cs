using Newtonsoft.Json;

namespace TuShan.BountyHunterDream.Setting.Common
{
    public class JsonUtil
    {
        private static JsonSerializerSettings _setting = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            DateFormatString = "yyyy-MM-dd HH:mm:ss"
        };

        /// <summary>
        /// 将JSON转换为指定类型的对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="json">json字符串</param>
        /// <returns></returns>
        public static T ToObject<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static string ToJson<T>(T t)
        {
            return ToJson<T>(t, _setting);
        }

        public static string ToJson<T>(T t, JsonSerializerSettings jsonSetting)
        {
            if (jsonSetting == null)
                jsonSetting = _setting;
            return InnerToJson(t, jsonSetting);
        }

        /// <summary>
        /// 生成JSON字符串
        /// </summary>
        /// <param name="obj">生成json的对象</param>
        /// <param name="formatjson">是否格式化</param>
        /// <returns></returns>
        private static string InnerToJson(object obj, JsonSerializerSettings jsonSetting)
        {
            var json = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented, jsonSetting);
            return json;
        }
    }
}
