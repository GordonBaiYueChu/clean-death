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
            Browsers = new List<StructBrowser>();
        }

        public List<StructCleanFolder> CleanFolders { get; set; }


        public List<StructBrowser> Browsers { get; set; }

        /// <summary>
        /// 最大超时时间（天）
        /// </summary>
        public int MaxTimeOutDay { get; set; } 

        /// <summary>
        /// 需要执行清楚的时间点
        /// </summary>
        public DateTime NeedCleanTime { get; set; }
    }
}
