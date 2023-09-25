using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TuShan.BountyHunterDream.Setting.Common;
using TuShan.BountyHunterDream.Setting.Struct;

namespace TuShan.BountyHunterDream.Setting.Setting
{
    public class SystemSetting : BaseSetting<SystemSetting>
    {
        public List<AppSetttingStruct> WindowApps { get; set; }
    }
}
