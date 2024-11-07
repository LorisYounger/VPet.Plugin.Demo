using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VPet.MutiPlayer.EatBean.Core
{
    /// <summary>
    /// 吃豆游戏核心
    /// </summary>
    public class EatBeanCore
    {
        public EatBeanCore(int width, int height)
        {
            Map = new Map(width, height);
        }
        /// <summary>
        /// 当前回合数
        /// </summary>
        public int Round { get; set; } = 1;
        /// <summary>
        /// 当前行动玩家剩余操作次数
        /// </summary>
        public int Turn { get; set; } = 2;

        /// <summary>
        /// 当前行动玩家
        /// </summary>
        public int NowPlayer { get; set; } = 0;

        public Map Map { get; set; }

        public List<Player> Players { get; set; } = new List<Player>();

        public List<EBObject> Objects { get; set; } = new List<EBObject>();
    }
}
