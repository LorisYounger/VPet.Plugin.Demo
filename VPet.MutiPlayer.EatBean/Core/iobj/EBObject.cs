using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VPet.MutiPlayer.EatBean.Core
{
    /// <summary>
    /// 所有物品
    /// </summary>
    public abstract class EBObject
    {
        /// <summary>
        /// 用于数据传输和定位的ID 唯一
        /// </summary>
        public long ebID { get; set; }

        /// <summary>
        /// 是否可以通过
        /// </summary>
        public abstract bool IsPassable { get; }

        /// <summary>
        /// 触发踩踏效果
        /// </summary>
        public abstract void OnTouch(Player player);

        /// <summary>
        /// X轴位置
        /// </summary>
        public int X { get; set; }
        /// <summary>
        /// Y轴位置
        /// </summary>
        public int Y { get; set; }


    }
}
