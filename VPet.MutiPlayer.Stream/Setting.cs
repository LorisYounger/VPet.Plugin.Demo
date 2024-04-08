using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VPet.MutiPlayer.Stream
{
    public class Setting
    {
        /// <summary>
        /// 允许聊天
        /// </summary>
        public bool AllowTalk { get; set; } = true;
        /// <summary>
        /// 允许显示名字
        /// </summary>
        public bool AllowName { get; set; } = true;
        /// <summary>
        /// 文本检查
        /// </summary>
        public bool WordsCheck { get; set; } = false;
        /// <summary>
        /// 文本替换
        /// </summary>
        public bool WorkReplace { get; set; } = false;
        /// <summary>
        /// 自定义敏感词
        /// </summary>
        public string DIYSensitive { get; set; } = "";
        /// <summary>
        /// 白名单加入
        /// </summary>
        public bool WhiteListJoin { get; set; } = false;
        /// <summary>
        /// 白名单聊天
        /// </summary>
        public bool WhiteListTalk { get; set; } = false;
        /// <summary>
        /// 白名单加入列表
        /// </summary>
        public string WhiteJoinList { get; set; } = "";
        /// <summary>
        /// 白名单聊天列表
        /// </summary>
        public string WhiteTalkList { get; set; } = "";

        public Setting() { }

    }
}
