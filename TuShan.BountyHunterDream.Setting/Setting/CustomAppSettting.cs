using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TuShan.BountyHunterDream.Setting.Common;
using TuShan.BountyHunterDream.Setting.Struct;

namespace TuShan.BountyHunterDream.Setting.Setting
{
    /// <summary>
    /// 用户自定义配置
    /// </summary>
    public class CustomAppSettting : BaseSetting<CustomAppSettting>
    {
        public CustomAppSettting() 
        {
        
        }

        /// <summary>
        /// 所有用户自定义app参数
        /// </summary>
        public List<AppSetttingStruct> AllCustomAppData { get; set; }
    }
}
