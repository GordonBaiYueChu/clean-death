using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TuShan.BountyHunterDream.Setting.Common
{
    public class BaseSetting<T> : BaseStruct<T>, ISettingStorage<T>
    {
        private JsonSettingStorage<T> Store;
        public BaseSetting()
        {
            Store = new JsonSettingStorage<T>();
        }

        public T Read(string FileName)
        {
            return Store.Read(FileName);
        }


        public void Write(T t, string FileName)
        {
            Store.Write(t, FileName);
        }


        public virtual bool CheckNull(T t)
        {
            return t == null;
        }
    }
}
