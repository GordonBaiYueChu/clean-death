using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TuShan.BountyHunterDream.Setting.Common
{
    interface ICompare<T>
    {
        bool ValueSame();
        bool ValueSame(T t, bool checkAll = false);
        bool ValueSameForValue(T t);
    }
}
