using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TuShan.BountyHunterDream.Setting.Common;

namespace TuShan.BountyHunterDream.Setting.Struct
{
    public class AppSetttingStruct : BaseStruct<AppSetttingStruct>
    {
        /// <summary>
        /// 软件exe名称，任务管理器中名称,为了关闭app（任务管理器中跟可执行exe文件可能不同，但是可以不区分大小写）
        /// </summary>
        public string AppExeName { get; set; }

        /// <summary>
        /// 软件名称
        /// </summary>
        public string AppDisplayName { get; set; }

        /// <summary>
        /// 软件文件地址(不包含exe)
        /// </summary>
        public string AppFilePath { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnable { get; set; }

    }
}
