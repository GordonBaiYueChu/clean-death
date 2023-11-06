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

        public List<StructCleanFolder> CleanFolders { get; set; }

        public List<AppSetttingStruct> CleanApps { get; set; }

        public int MaxTimeOutDay { get; set; } 

        public DateTime NeedCleanTime { get; set; }

        public int WriteTime { get; set; }

    }
}
