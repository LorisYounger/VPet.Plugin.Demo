using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VPet.MutiPlayer.EatBean
{
    /// <summary>
    /// 用于传输用的数据
    /// </summary>
    public class TransData
    {
        ///// <summary>
        ///// 传输数据编号, 序列递增, 用于检测是否有丢包
        ///// </summary>
        //public int TransIndex { get; set; } = 1000;
        /// <summary>
        /// 指令类型
        /// </summary>
        public enum OrderType
        {
            /// <summary>
            /// 创建新游戏空地图
            /// </summary>
            NewGame,

            /// <summary>
            /// 寻求自定id的传输数据
            /// </summary>
            AskTransData,

            /// <summary>
            /// 添加新物品/玩家
            /// </summary>
            AddEBObject,
        }
        /// <summary>
        /// 传输的数据
        /// </summary>
        public string Data { get; set; } = string.Empty;

        /// <summary>
        /// 新游戏数据
        /// </summary>
        public class NewGameData
        {

        }
    }
}
