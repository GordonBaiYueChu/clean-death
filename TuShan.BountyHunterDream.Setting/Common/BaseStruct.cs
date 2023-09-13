using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TuShan.BountyHunterDream.Setting.Common
{
    public class BaseStruct<T> : IClone<T>, ICompare<T>
    {
        static JsonSerializerSettings _setting = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            DateFormatString = "yyyy-MM-dd HH:mm:ss",
            ContractResolver = new LimitPropsContractResolver(new string[] { "Guid" }, false)
        };
        public BaseStruct()
        {
        }

        public T Clone()
        {
            T t;
            string jsonStr = JsonUtil.ToJson(this);
            t = JsonUtil.ToObject<T>(jsonStr);
            BaseStruct<T> bt = t as BaseStruct<T>;
            return t;
        }

        public virtual bool ValueSame()
        {
            throw new NotImplementedException();
        }

        public virtual bool ValueSame(T t, bool checkAll = false)
        {
            string jsonStr = JsonUtil.ToJson(this, _setting);
            string jsonStrT = JsonUtil.ToJson(t, _setting);
            if (jsonStr == jsonStrT)
                return true;
            else
                return false;

        }
        public virtual bool ValueSame(T t, string[] props)
        {
            string jsonStr = JsonUtil.ToJson(this, _setting);
            string jsonStrT = JsonUtil.ToJson(t, _setting);
            if (jsonStr == jsonStrT)
                return true;
            else
                return false;

        }

        public bool ValueSameForValue(T t)
        {
            bool isSame = true;
            PropertyInfo[] props = this.GetType().GetProperties();
            foreach (PropertyInfo item in props)
            {
                object obj = item.GetValue(this);
                if (obj != null)
                {
                    if (obj is ICompare<T>)
                    {
                        ICompare<T> objComp = obj as ICompare<T>;

                        T objT = (T)item.GetValue(t);
                        if (objT != null)
                        {
                            isSame = objComp.ValueSame(objT);
                            if (!isSame)
                            {
                                return isSame;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else if (obj is IList<T>)
                    {
                        IList<T> coll = obj as IList<T>;
                        var objT = item.GetValue(t);
                        if (objT != null)
                        {
                            IList<T> collT = objT as IList<T>;
                            if (coll.Count == collT.Count)
                            {
                                for (int i = 0; i < coll.Count; i++)
                                {
                                    if (coll[i] is ICompare<T>)
                                    {
                                        ICompare<T> objComp = coll[i] as ICompare<T>;
                                        isSame = objComp.ValueSame(collT[i]);
                                        if (!isSame)
                                        {
                                            return isSame;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            return isSame;

        }
    }
}
