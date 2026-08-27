using LinePutScript.Converter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VPet.Plugin.VPetTTS
{
    public class Setting
    {
        /// <summary>
        /// 语速
        /// </summary>
        [Line]
        public double Rate
        {
            get => rate; set
            {
                rate = value;
                RateStr = $"{(value >= 0 ? "+" : "")}{value:f2}%";
            }
        }
        private double rate = 0;
        public string RateStr { get; private set; } = "+0%";
        double pitch = 10;

        /// <summary>
        /// 音调
        /// </summary>
        [Line]
        public double Pitch
        {
            get
            {
                return pitch;
            }
            set
            {
                pitch = value;
                PitchStr = $"{(value >= 0 ? "+" : "")}{value:f2}Hz";
            }
        }

        public string PitchStr { get; private set; } = "+10Hz";
        /// <summary>
        /// 讲述人
        /// </summary>
        [Line]
        public string Speaker { get; set; }
        /// <summary>
        /// 启用EdgeTTS
        /// </summary>
        [Line]
        public bool Enable { get; set; } = true;

        /// <summary>
        /// 括号过滤设置（括号内的动作描写只显示不朗读）
        /// </summary>
        [Line]
        public TextFilterSetting TextFilter { get; set; } = new TextFilterSetting();
    }

    /// <summary>
    /// 朗读文本过滤设置。
    ///
    /// </summary>
    public class TextFilterSetting
    {
        /// <summary>
        /// 总开关。关闭时文本原样朗读
        /// </summary>
        [Line]
        public bool Enable { get; set; } = false;

        /// <summary>圆括号 ( ) （ ）</summary>
        [Line]
        public bool RoundBracket { get; set; } = true;

        /// <summary>方括号 [ ] 【 】 〔 〕</summary>
        [Line]
        public bool SquareBracket { get; set; } = true;

        /// <summary>花括号 { } ｛ ｝</summary>
        [Line]
        public bool CurlyBracket { get; set; } = false;

        /// <summary>
        /// 尖括号 < >; ＜ ＞。默认关闭：大于小于号会被误伤
        /// </summary>
        [Line]
        public bool AngleBracket { get; set; } = false;

        /// <summary>
        /// 书名号 《 》 〈 〉。默认关闭：书名不该被吞掉。
        /// 单独一项而不并进 <see cref="AngleBracket"/>，
        /// 是因为「念书名」和「念大于小于号」是两回事
        /// </summary>
        [Line]
        public bool BookTitleMark { get; set; } = false;

        /// <summary>
        /// 成对标签 <动作>;轻轻摸了摸主人的头</动作>;，连标签带内容一起跳过。
        /// 首尾同名才算，所以「5 < x and y >; 3」不会被误伤
        /// </summary>
        [Line]
        public bool PairedTag { get; set; } = true;

        /// <summary>成对星号包裹的动作描写 *摸摸头* / **摸摸头**</summary>
        [Line]
        public bool Asterisk { get; set; } = true;

        /// <summary>
        /// 自定义括号对，开闭两个字符为一组，组间可用空格或逗号分隔。
        /// 例如「」『』
        /// </summary>
        [Line]
        public string CustomPairs { get; set; } = "";

        /// <summary>
        /// 自定义正则，每行一条，命中的内容不朗读；以 # 开头的行是注释。
        /// 写错的那条会被跳过，不影响其他规则
        /// </summary>
        [Line]
        public string CustomRegex { get; set; } = "";

        /// <summary>
        /// 复制一份，供设置窗口改着玩；用户不点「保存设置」就不保存
        /// </summary>
        public TextFilterSetting Clone() => new TextFilterSetting
        {
            Enable = Enable,
            RoundBracket = RoundBracket,
            SquareBracket = SquareBracket,
            CurlyBracket = CurlyBracket,
            AngleBracket = AngleBracket,
            BookTitleMark = BookTitleMark,
            PairedTag = PairedTag,
            Asterisk = Asterisk,
            CustomPairs = CustomPairs,
            CustomRegex = CustomRegex
        };

        /// <summary>
        /// 把另一份设置的取值搬过来（保持对象引用不变）
        /// </summary>
        public void CopyFrom(TextFilterSetting other)
        {
            if (other == null)
                return;

            Enable = other.Enable;
            RoundBracket = other.RoundBracket;
            SquareBracket = other.SquareBracket;
            CurlyBracket = other.CurlyBracket;
            AngleBracket = other.AngleBracket;
            BookTitleMark = other.BookTitleMark;
            PairedTag = other.PairedTag;
            Asterisk = other.Asterisk;
            CustomPairs = other.CustomPairs ?? "";
            CustomRegex = other.CustomRegex ?? "";
        }
    }
}
