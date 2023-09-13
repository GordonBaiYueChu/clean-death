using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TuShan.BountyHunterDream.Setting.Common
{
    public interface IClone<T>
    {
        T Clone();
    }
}
