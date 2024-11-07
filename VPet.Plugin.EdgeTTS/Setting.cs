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
        /// 加密秘钥
        /// </summary>
        /// 一个空格为默认值空,这时候可以根据语言自动使用默认值
        [Line]
        public string Sec_MS_GEC_URL { get; set; } = " ";
    }
}
