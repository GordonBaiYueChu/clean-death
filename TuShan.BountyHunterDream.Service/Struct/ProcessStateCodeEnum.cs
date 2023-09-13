using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TuShan.BountyHunterDream.Service.Struct
{
    /// <summary>
    /// 程序关闭返回值
    /// </summary>
    public enum ProcessStateCodeEnum
    {
        /// <summary>
        /// 正常关闭
        /// </summary>
        ExitCodeSuccess = -111,
        /// <summary>
        /// 未知关闭
        /// </summary>
        ExitCodeUnKnown = -1000
    }
}
