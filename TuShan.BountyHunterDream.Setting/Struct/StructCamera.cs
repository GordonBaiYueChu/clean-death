using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TuShan.BountyHunterDream.Setting.Common;

namespace TuShan.BountyHunterDream.Setting.Struct
{
    public class StructCleanFolder : BaseStruct<StructCleanFolder>
    {
        public string FolderPath { get; set; }

        public bool IsEnable { get; set; }
    }

    public class StructBrowser : BaseStruct<StructBrowser>
    {
        public string BrowserName { get; set; }
    }

}
