using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VPet.Plugin.MutiRedEnvelope
{
    /// <summary>
    /// 红包消息数据类，用于存储红包的相关信息
    /// </summary>
    public class RedMessageData
    {
        /// <summary>
        /// 红包ID，唯一标识一个红包
        /// </summary>
        public int REDID { get; set; }


        /// <summary>
        /// 发送红包的玩家id
        /// </summary>
        public ulong SenderID { get; set; }
        /// <summary>
        /// 金额
        /// </summary>
        public int Money { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public int Count { get; set; }
        /// <summary>
        /// 祝福/拼手气
        /// </summary>
        public string Message { get; set; } = "";
        /// <summary>
        /// 是否是玩具钞
        /// </summary>
        public bool IsFake { get; set; } = false;

        public enum RedMessageType
        {
            /// <summary>
            /// 拼手气(随机)红包
            /// </summary>
            Random,
            /// <summary>
            /// 普通(平分)红包
            /// </summary>
            Normal,
            /// <summary>
            /// 指定(单人)红包
            /// </summary>
            Someone,
            /// <summary>
            /// 口令红包
            /// </summary>
            Talk
        }
        /// <summary>
        /// 目标玩家id，指定红包使用
        /// </summary>
        public ulong TargetID { get; set; } = 0;
        /// <summary>
        /// 红包类型
        /// </summary>
        public RedMessageType Type { get; set; }

        public class GetData
        {
            /// <summary>
            /// 领取红包的玩家id
            /// </summary>
            public ulong GetterID { get; set; }
            /// <summary>
            /// 领取金额
            /// </summary>
            public double Money { get; set; }
            /// <summary>
            /// 获取时间
            /// </summary>
            public DateTime GetTime { get; set; }
        }
        /// <summary>
        /// 剩余金额，领取后更新
        /// </summary>
        public double LeftMoney { get; set; }
    }
}
