using LinePutScript.Converter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VPet.Plugin.DoingDisplay
{

    public class Setting
    {
        public List<SoftwareStatistical> Statisticals { get; set; } = new List<SoftwareStatistical>();
        public List<DailyStatistical> Dailies { get; set; } = new List<DailyStatistical>();
        /// <summary>
        /// 统计信息
        /// </summary>
        public class SoftwareStatistical
        {
            /// <summary>
            /// 软件进程名字
            /// </summary>
            public string SoftWareProcessName { get; set; } = string.Empty;
            /// <summary>
            /// 软件名字
            /// </summary>
            public string SoftWare { get; set; } = string.Empty;
            /// <summary>
            /// 是否编辑过
            /// </summary>
            public bool isEdit { get; set; } = false;
            /// <summary>
            /// 使用时长 (分钟)
            /// </summary>
            public int Minute { get; set; }
            /// <summary>
            /// 最近使用时间
            /// </summary>
            public DateTime LastUse { get; set; }
        }
        public class DailyStatistical
        {
            /// <summary>
            /// 日期
            /// </summary>
            public DateTime Date { get; set; }
            /// <summary>
            /// 使用时长 (分钟)
            /// </summary>
            public int Minute { get; set; }
            /// <summary>
            /// 软件使用情况, key为软件名字, value为使用时长(分钟)
            /// </summary>
            public Dictionary<string, int> SoftwareUsage { get; set; } = new Dictionary<string, int>();
        }
    }
}
