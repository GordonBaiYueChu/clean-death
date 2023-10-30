using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TuShan.BountyHunterDream.Setting.Common;
using TuShan.BountyHunterDream.Setting.Struct;

namespace TuShan.BountyHunterDream.Setting.Setting
{
    public class CleanDeathSetting : BaseSetting<CleanDeathSetting>
    {
        public CleanDeathSetting()
        {
            CleanFolders = new List<StructCleanFolder>();
            CleanApps = new List<AppSetttingStruct>();
        }

        /// <summary>
        /// 文件夹信息
        /// </summary>
        public List<StructCleanFolder> CleanFolders { get; set; }

        /// <summary>
        /// windows应用
        /// </summary>
        public List<AppSetttingStruct> CleanApps { get; set; }

        /// <summary>
        /// 最大超时时间（天）
        /// </summary>
        public int MaxTimeOutDay { get; set; } 

        /// <summary>
        /// 需要执行清除的时间点
        /// </summary>
        public DateTime NeedCleanTime { get; set; }

        /// <summary>
        /// 写入脏数据的次数
        /// </summary>
        public int WriteTime { get; set; }

    }
}
