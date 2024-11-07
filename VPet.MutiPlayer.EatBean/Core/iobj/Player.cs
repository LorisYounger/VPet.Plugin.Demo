using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VPet.MutiPlayer.EatBean.Core
{
    public class Player : EBObject
    {
        public Player(string name)
        {
            Name = name;
        }

        /// <summary>
        /// 是否失败
        /// </summary>
        public bool IsFail { get; set; } = false;

        /// <summary>
        /// 玩家名字/ID 用于定位玩家
        /// </summary>
        public string Name { get; set; }

        public double hp;
        public double HP
        {
            get => hp;
            set
            {
                if (value > HPMax)
                {
                    hp = HPMax;
                }
                else if (value < 0)
                {
                    Handle_EmptyHP?.Invoke();
                }
                else
                {
                    hp = value;
                }
            }
        }
        public double HPMax { get; set; } = 100;


        public double MP { get; set; } = 20;
        public double MPMax { get; set; } = 20;

        public double EXP { get; set; } = 0;

        public int Level { get; set; }

        public double Attack { get; set; }

        public double Defence { get; set; }

        public override bool IsPassable => false;

        public override void OnTouch(Player player) { }

        /// <summary>
        /// HP为空事件
        /// </summary>
        public Action Handle_EmptyHP;

    }
}
