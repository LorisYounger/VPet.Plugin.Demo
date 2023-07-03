using LinePutScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VPet.Plugin.DemoClock
{
    public class Setting : Line
    {
        public Setting() : base("DemoClock", "")
        {
            timeshifting = 0;
            hour24 = false;
            tomato_worktime = 45;
            tomato_resttime = 15;
            tomato_resttimelong = 45;
        }
        public Setting(ILine line) : base(line)
        {
            timeshifting = GetFloat("timeshifting", 0);
            hour24 = GetBool("hour24");
            tomato_worktime = GetDouble("tomato_worktime", 45);
            tomato_resttime = GetDouble("tomato_resttime", 15);
            tomato_resttimelong = GetDouble("tomato_resttimelong", 45);
        }
        double timeshifting;
        /// <summary>
        /// 时间偏移
        /// </summary>
        public double TimeShifting
        {
            get => timeshifting;
            set
            {
                timeshifting = value;
                SetFloat("timeshifting", value);
            }
        }
        /// <summary>
        /// 是否自动放置到桌宠后方
        /// </summary>
        public bool PlaceAutoBack
        {
            get => !GetBool("placeautofront");
            set => SetBool("placeautofront", !value);
        }
        ///// <summary>
        ///// 是否启用
        ///// </summary>
        //public bool TurnON
        //{
        //    get => !GetBool("turnon");
        //    set => SetBool("turnon", !value);
        //}
        /// <summary>
        /// 放置距离设置
        /// </summary>
        public double PlaceTop
        {
            get => GetDouble("placetop", 400);
            set => SetDouble("placetop", value);
        }
        /// <summary>
        /// 默认倒计时
        /// </summary>
        public double DefaultCountDown
        {
            get => GetDouble("defaultcountdown", 5);
            set => SetDouble("defaultcountdown", value);
        }
        /// <summary>
        /// 闲置透明度 (0-1.0)
        /// </summary>
        public double Opacity
        {
            get => GetDouble("opacity", 0.6);
            set => SetDouble("opacity", value);
        }
        bool hour24;
        /// <summary>
        /// 是否使用24小时制
        /// </summary>
        public bool Hour24
        {
            get => hour24;
            set
            {
                hour24 = value;
                SetBool("hour24", value);
            }
        }
        double tomato_worktime;
        /// <summary>
        /// 番茄钟工作时长
        /// </summary>
        public double Tomato_WorkTime
        {
            get => tomato_worktime;
            set
            {
                tomato_worktime = value;
                SetDouble("tomato_worktime", value);
            }
        }
        double tomato_resttime;
        /// <summary>
        /// 番茄钟休息时长
        /// </summary>
        public double Tomato_RestTime
        {
            get => tomato_resttime;
            set { tomato_resttime = value; SetDouble("tomato_resttime", value); }
        }
        public double tomato_resttimelong;
        /// <summary>
        /// 番茄钟长休息时长
        /// </summary>
        public double Tomato_RestTimeLong
        {
            get => tomato_resttimelong;
            set { SetDouble("tomato_resttimelong", value); tomato_resttimelong = value; }
        }
        /// <summary>
        /// 番茄钟工作语音
        /// </summary>
        public string Tomato_WorkVoice
        {
            get => GetString("tomato_workvoice", "pack://application:,,,/Res/Work.mp3");
            set => SetString("tomato_workvoice", value);
        }
        /// <summary>
        /// 番茄钟休息语音
        /// </summary>
        public string Tomato_RestVoice
        {
            get => GetString("tomato_restvoice", "pack://application:,,,/Res/Rest.mp3");
            set => SetString("tomato_restvoice", value);
        }
        /// <summary>
        /// 番茄钟结束语音
        /// </summary>
        public string Tomato_EndVoice
        {
            get => GetString("tomato_endvoice", "pack://application:,,,/Res/End.mp3");
            set => SetString("tomato_endvoice", value);
        }
        /// <summary>
        /// 番茄钟工作语音
        /// </summary>
        public string CountDownVoice
        {
            get => GetString("countdownvoice", "pack://application:,,,/Res/CountDown.mp3");
            set => SetString("countdownvoice", value);
        }
        /// <summary>
        /// 拥有番茄数量
        /// </summary>
        public int Tomato_Count
        {
            get => GetInt("tomato_count");
            set => SetInt("tomato_count", value);
        }
        /// <summary>
        /// 拥有番茄总数量
        /// </summary>
        public int Tomato_Count_Total
        {
            get => GetInt("tomato_count_total");
            set => SetInt("tomato_count_total", value);
        }
        /// <summary>
        /// 添加番茄数量
        /// </summary>
        public void AddTomato(int count)
        {
            Tomato_Count += count;
            Tomato_Count_Total += count;
        }
    }
}
