using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TuShan.BountyHunterDream.Setting.Common;

namespace TuShan.BountyHunterDream.Setting.Struct
{
    public class CustomAppSetttingStruct : BaseStruct<CustomAppSetttingStruct>
    {
        /// <summary>
        /// 软件exe名称，任务管理器中名称
        /// </summary>
        public string AppExeName { get; set; }

        /// <summary>
        /// 软件名称
        /// </summary>
        public string AppDisplayName { get; set; }

        /// <summary>
        /// 软件文件地址
        /// </summary>
        public string AppFilePath { get; set; }

        /// <summary>
        /// 卸载命令
        /// </summary>
        public string UnInstallString { get; set; }

    }
}
