﻿using Panuon.WPF.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static VPet.Plugin.DemoClock.DemoClock;
using System.Timers;
using LinePutScript.Localization.WPF;
using System.Threading.Tasks.Sources;
using System.Text.RegularExpressions;
using System.IO;

namespace VPet.Plugin.DemoClock
{
    /// <summary>
    /// TimeClock.xaml 的交互逻辑
    /// </summary>
    public partial class TimeClock : UserControl
    {
        DemoClock Master;
        public DispatcherTimer TimeTimer;
        public DispatcherTimer CountTimer;
        public DateTime StartTime;
        public TimeSpan PauseTime;
        public bool IsPause = false;
        Timer CloseTimer;

        public TimeClock(DemoClock master)
        {
            InitializeComponent();
            Resources = Application.Current.Resources;

            Master = master;
            Master.MW.Main.UIGrid.Children.Insert(0, this);

            TimeTimer = new DispatcherTimer();
            TimeTimer.Interval = TimeSpan.FromSeconds(1);
            TimeTimer.Tick += TimeTimer_Tick;
            TimeTimer.Start();

            CountTimer = new DispatcherTimer();
            //CountTimer.Interval = TimeSpan.FromSeconds(1);
            CountTimer.Tick += CountTimer_Tick;

            CloseTimer = new Timer()
            {
                Interval = 4000,
                AutoReset = false,
                Enabled = false
            };
            CloseTimer.Elapsed += CloseTimer_Elapsed;

            Opacity = master.Set.Opacity;
            Margin = new Thickness(0, master.Set.PlaceTop, 0, 0);
            Master.MW.Main.MouseEnter += UserControl_MouseEnter;
            Master.MW.Main.MouseLeave += UserControl_MouseLeave;
            TimeTimer_Tick();
            WeatherControl.OnWeatherPageShow += WeatherControl_OnWeatherPageShow;

        }

        private void WeatherControl_OnWeatherPageShow()
        {
            if (Master.weatherWindow != null)
            {
                Master.MW.Dispatcher.BeginInvoke(new Action(() =>
                {
                    Master.weatherWindow.Close();
                    Master.weatherWindow = new winWeatherPage(Master);
                    Master.weatherWindow.Show();
                }));
                return;
            }
            Master.weatherWindow = new winWeatherPage(Master);
            Master.MW.Dispatcher.BeginInvoke(new Action(() => { Master.weatherWindow.Show(); }));
        }

        private void CloseTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Opacity = Master.Set.Opacity;
                if (Master.Set.PlaceAutoBack && Master.MW.Main.UIGrid.Children.Contains(this))
                {
                    Master.MW.Main.UIGrid.Children.Remove(this);
                    Master.MW.Main.UIGrid_Back.Children.Add(this);
                }
            });
        }
        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Master.Set.PlaceAutoBack && Master.MW.Main.UIGrid_Back.Children.Contains(this))
            {
                Master.MW.Main.UIGrid_Back.Children.Remove(this);
                Master.MW.Main.UIGrid.Children.Insert(0, this);
            }
            Opacity = 0.95;
            CloseTimer.Enabled = false;
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            CloseTimer.Start();
        }

        bool TimeSpanChanged = false;
        private void CountTimer_Tick(object sender = null, EventArgs e = null)
        {
            if (IsPause)
                return;
            if (Master.mode != Mode.None)
            {
                WeatherControl.Visibility = Visibility.Collapsed;
                TTimes.Visibility = Visibility.Visible;
                TDates.Visibility = Visibility.Visible;
                TDayofWeek.Visibility = Visibility.Visible;
            }
            switch (Master.mode)
            {
                default:
                case Mode.None:
                    CountTimer.Stop();
                    break;
                case Mode.Timing:
                    var diff = DateTime.Now - StartTime + PauseTime;
                    if (diff.TotalMinutes > 1)
                    {
                        TTimes.Text = diff.ToString(@"mm\:ss");
                        if (TimeSpanChanged)
                        {
                            CountTimer.Interval = TimeSpan.FromSeconds(1);
                            TimeSpanChanged = false;
                        }
                    }
                    else
                        TTimes.Text = diff.ToString(@"ss\:ff");

                    if (diff.TotalMinutes < 1.5)
                    {
                        TDates.Text = "计时: {0:f1} 秒".Translate(diff.TotalSeconds);
                    }
                    else if (diff.TotalHours < 1.5)
                    {
                        TDates.Text = "计时: {0:f1} 分钟".Translate(diff.TotalMinutes);
                    }
                    else
                    {
                        TDates.Text = "计时: {0:f1} 小时".Translate(diff.Hours);
                    }
                    break;
                case Mode.CountDown:
                    diff = StartTime - DateTime.Now;
                    if (diff <= TimeSpan.Zero)
                    {
                        TTimes.Text = "时间到".Translate();
                        TDates.Text = "计时结束".Translate();
                        TOthers.Text = "点击此处回到时间显示".Translate();
                        TOthers.Visibility = Visibility.Visible;
                        Master.mode = Mode.CountDown_End;
                        CountTimer.Stop();
                        //**********在此处添加计时结束逻辑**********//
                        var voicetext = "";
                        if (Tools.TryGetInputTypeAndValue(Master.Set.CountDownVoice, out voicetext))
                        {
                            MessageBox.Show(voicetext);
                            Master.musicPlayer.Play(voicetext);
                        }
                        else
                        {
                            Master.MW.Dispatcher.BeginInvoke(() => { Master.MW.Main.SayRnd(voicetext, true); });
                        }
                        //**********在此处添加计时结束逻辑**********//
                        return;
                    }
                    PBTimeLeft.Value = PBTimeLeft.Maximum - diff.TotalMinutes;
                    if (diff.TotalMinutes < 1)
                    {
                        TTimes.Text = diff.ToString(@"ss\:ff");
                        if (TimeSpanChanged)
                        {
                            CountTimer.Interval = TimeSpan.FromMilliseconds(50);
                            TimeSpanChanged = false;
                        }
                    }
                    else
                    {
                        TTimes.Text = diff.ToString(@"mm\:ss");
                    }
                    if (diff.TotalMinutes < 1.5)
                    {
                        TDates.Text = "剩余: {0:f1} 秒".Translate(diff.TotalSeconds);
                    }
                    else if (diff.TotalHours < 1.5)
                    {
                        TDates.Text = "剩余: {0:f1} 分钟".Translate(diff.TotalMinutes);
                    }
                    else
                    {
                        TDates.Text = "剩余: {0:f1} 小时".Translate(diff.Hours);
                    }
                    break;
                case Mode.Tomato_Work:
                    diff = DateTime.Now - StartTime;
                    var diffleft = TimeSpan.FromMinutes(Master.Set.Tomato_WorkTime) - diff;
                    if (diffleft <= TimeSpan.Zero)
                    {
                        Master.Set.AddTomato((int)Master.Set.Tomato_WorkTime / 10);
                        Master.MW.Core.Save.Money += (int)Master.Set.Tomato_WorkTime;
                        TTimes.Text = "时间到".Translate();
                        TDates.Text = "工作结束".Translate();
                        TOthers.Text = "点击此处开始休息".Translate();
                        Master.mode = Mode.CountDown_End;
                        CountTimer.Stop();
                        //**********在此处添加工作结束逻辑**********//
                        var replacements = new Dictionary<string, string>
                        {
                            { "Mode", "Work".Translate() },
                            { "模式", "工作".Translate() }
                        };
                        var voicetext = "";
                        if (Tools.TryGetInputTypeAndValue(Master.Set.Tomato_EndVoice, out voicetext))
                        {
                            Master.musicPlayer.Play(voicetext);
                        }
                        else
                        {
                            voicetext = Tools.ReplacePlaceholders(voicetext, replacements);
                            Master.MW.Dispatcher.Invoke(() => { Master.MW.Main.SayRnd(voicetext, true); });
                        }
                        //**********在此处添加工作结束逻辑**********//
                        return;
                    }
                    TTimes.Text = diff.TotalMinutes.ToString("f1") + 'm';
                    PBTimeLeft.Value = diff.TotalMinutes;
                    if (diffleft.TotalMinutes < 1.5)
                    {
                        TDates.Text = "工作剩{0:f1}秒".Translate(diffleft.TotalSeconds);
                    }
                    else if (diffleft.TotalHours < 1.5)
                    {
                        TDates.Text = "工作剩{0:f1}分钟".Translate(diffleft.TotalMinutes);
                    }
                    else
                    {
                        TDates.Text = "工作剩{0:f1}小时".Translate(diffleft.Hours);
                    }
                    break;
                case Mode.Tomato_Rest:
                    diff = DateTime.Now - StartTime;
                    diffleft = TimeSpan.FromMinutes(Master.Set.Tomato_RestTime) - diff;
                    if (diffleft <= TimeSpan.Zero)
                    {
                        TTimes.Text = "时间到".Translate();
                        TDates.Text = "休息结束".Translate();
                        TOthers.Text = "点击此处开始工作".Translate();
                        Master.mode = Mode.CountDown_End;
                        CountTimer.Stop();
                        //**********在此处添加休息结束逻辑**********//
                        var replacements = new Dictionary<string, string>
                        {
                            { "Mode", "Rest".Translate() },
                            { "模式", "休息".Translate() }
                        };
                        var voicetext = "";
                        if (Tools.TryGetInputTypeAndValue(Master.Set.Tomato_EndVoice, out voicetext))
                        {
                            Master.musicPlayer.Play(voicetext);
                        }
                        else
                        {
                            voicetext = Tools.ReplacePlaceholders(voicetext, replacements);
                            Master.MW.Dispatcher.BeginInvoke(() => { Master.MW.Main.SayRnd(voicetext, true); });
                        }
                        //**********在此处添加休息结束逻辑**********//
                        return;
                    }
                    TTimes.Text = diff.TotalMinutes.ToString("f1") + 'm';
                    PBTimeLeft.Value = diff.TotalMinutes;

                    if (diffleft.TotalMinutes < 1.5)
                    {
                        TDates.Text = "休息剩{0:f1}秒".Translate(diffleft.TotalSeconds);
                    }
                    else if (diffleft.TotalHours < 1.5)
                    {
                        TDates.Text = "休息剩{0:f1}分钟".Translate(diffleft.TotalMinutes);
                    }
                    else
                    {
                        TDates.Text = "休息剩{0:f1}小时".Translate(diffleft.Hours);
                    }
                    break;
                case Mode.Tomato_Rest_Long:
                    diff = DateTime.Now - StartTime;
                    diffleft = TimeSpan.FromMinutes(Master.Set.Tomato_RestTimeLong) - diff;
                    if (diffleft <= TimeSpan.Zero)
                    {
                        TTimes.Text = "时间到".Translate();
                        TDates.Text = "长休息结束".Translate();
                        TOthers.Text = "点击此处开始工作".Translate();
                        Master.mode = Mode.CountDown_End;
                        CountTimer.Stop();
                        //**********在此处添加长休息结束逻辑**********//
                        var replacements = new Dictionary<string, string>
                        {
                            { "Mode", "Long Rest".Translate() },
                            { "模式", "长休息".Translate() }
                        };
                        var voicetext = "";
                        if (Tools.TryGetInputTypeAndValue(Master.Set.Tomato_EndVoice, out voicetext))
                        {
                            Master.musicPlayer.Play(voicetext);
                        }
                        else
                        {
                            voicetext = Tools.ReplacePlaceholders(voicetext, replacements);
                            Master.MW.Dispatcher.BeginInvoke(() => { Master.MW.Main.SayRnd(voicetext, true); });
                        }
                        //**********在此处添加长休息结束逻辑**********//
                        return;
                    }
                    TTimes.Text = diff.TotalMinutes.ToString("f1") + 'm';
                    PBTimeLeft.Value = diff.TotalMinutes;

                    if (diffleft.TotalMinutes < 1.5)
                    {
                        TDates.Text = $"休息剩{diffleft.TotalSeconds:f1}秒";
                    }
                    else if (diffleft.TotalHours < 1.5)
                    {
                        TDates.Text = $"休息剩{diffleft.TotalMinutes:f1}分钟";
                    }
                    else
                    {
                        TDates.Text = $"休息剩{diffleft.Hours:f1}小时";
                    }
                    break;
            }
        }
        /// <summary>
        /// 开始计时模式
        /// </summary>
        public void StartTiming()
        {
            StartTime = DateTime.Now;
            TimeSpanChanged = true;
            TOthers.Visibility = Visibility.Collapsed;
            IsPause = false;
            PauseTime = TimeSpan.Zero;
            CountTimer.Interval = TimeSpan.FromMilliseconds(50);
            CountTimer.Start();
            Master.mode = Mode.Timing;
        }
        /// <summary>
        /// 暂停计时模式
        /// </summary>
        public void PauseTiming()
        {
            IsPause = true;
            CountTimer.IsEnabled = false;
            PauseTime += DateTime.Now - StartTime;
            TDates.Text = "计时暂停".Translate() + TDates.Text.Substring(3);
        }
        /// <summary>
        /// 继续计时模式
        /// </summary>
        public void ContinueTiming()
        {
            StartTime = DateTime.Now;
            IsPause = false;
            CountTimer.Start();
            CountTimer_Tick();
        }
        /// <summary>
        /// 开始倒计时模式
        /// </summary>
        /// <param name="time">倒计时时长</param>
        public void StartCountDown(TimeSpan time)
        {
            Master.mode = Mode.CountDown;
            StartTime = DateTime.Now + time;
            TimeSpanChanged = true;
            IsPause = false;
            TOthers.Visibility = Visibility.Collapsed;
            PauseTime = TimeSpan.Zero;
            PBTimeLeft.Value = 0;
            PBTimeLeft.Maximum = time.TotalMinutes;
            PBTimeLeft.Visibility = Visibility.Visible;
            CountTimer.Interval = TimeSpan.FromSeconds(1);
            CountTimer.Start();
        }
        /// <summary>
        /// 继续倒计时
        /// </summary>
        public void ContinueCountDown()
        {
            StartTime = DateTime.Now + PauseTime;
            IsPause = false;
            CountTimer.Start();
            CountTimer_Tick();
        }
        /// <summary>
        /// 暂停倒计时
        /// </summary>
        public void PauseCountDown()
        {
            IsPause = true;
            CountTimer.IsEnabled = false;
            PauseTime = StartTime - DateTime.Now;
        }
        /// <summary>
        /// 开始工作
        /// </summary>
        public void StartWork()
        {
            Master.mode = Mode.Tomato_Work;
            StartTime = DateTime.Now;
            IsPause = false;
            TOthers.Visibility = Visibility.Visible;
            TOthers.Text = "番茄点数 {0} 累计点数 {1}".Translate(Master.Set.Tomato_Count, Master.Set.Tomato_Count_Total);
            PauseTime = TimeSpan.Zero;
            PBTimeLeft.Value = 0;
            PBTimeLeft.Visibility = Visibility.Visible;
            PBTimeLeft.Maximum = Master.Set.Tomato_WorkTime;
            CountTimer.Interval = TimeSpan.FromSeconds(1);
            CountTimer.Start();
            var voicetext = "";
            if (Tools.TryGetInputTypeAndValue(Master.Set.Tomato_WorkVoice, out voicetext))
            {
                Master.musicPlayer.Play(voicetext);
            }
            else
            {
                Master.MW.Dispatcher.BeginInvoke(() => { Master.MW.Main.SayRnd(voicetext, true); });
            }
        }
        /// <summary>
        /// 开始休息
        /// </summary>
        public void StartRest()
        {
            Master.mode = Mode.Tomato_Rest;
            StartTime = DateTime.Now;
            IsPause = false;
            TOthers.Visibility = Visibility.Visible;
            TOthers.Text = "番茄点数 {0} 累计点数 {1}".Translate(Master.Set.Tomato_Count, Master.Set.Tomato_Count_Total);
            PauseTime = TimeSpan.Zero;
            PBTimeLeft.Value = 0;
            PBTimeLeft.Visibility = Visibility.Visible;
            PBTimeLeft.Maximum = Master.Set.Tomato_RestTime;
            CountTimer.Interval = TimeSpan.FromSeconds(1);
            CountTimer.Start();
            var voicetext = "";
            if (Tools.TryGetInputTypeAndValue(Master.Set.Tomato_RestVoice, out voicetext))
            {
                Master.musicPlayer.Play(voicetext);
            }
            else
            {
                Master.MW.Dispatcher.BeginInvoke(() => { Master.MW.Main.SayRnd(voicetext, true); });
            }
        }
        /// <summary>
        /// 开始长休息
        /// </summary>
        public void StartRestLong()
        {
            Master.mode = Mode.Tomato_Rest_Long;
            StartTime = DateTime.Now;
            IsPause = false;
            TOthers.Visibility = Visibility.Visible;
            TOthers.Text = "番茄点数 {0} 累计点数 {1}".Translate(Master.Set.Tomato_Count, Master.Set.Tomato_Count_Total);
            PauseTime = TimeSpan.Zero;
            PBTimeLeft.Value = 0;
            PBTimeLeft.Visibility = Visibility.Visible;
            PBTimeLeft.Maximum = Master.Set.Tomato_RestTimeLong;
            CountTimer.Interval = TimeSpan.FromSeconds(1);
            CountTimer.Start();
            var voicetext = "";
        }
        /// <summary>
        /// 时钟模式UI更新
        /// </summary>
        private void TimeTimer_Tick(object sender = null, EventArgs e = null)
        {
            //相关UI更新
            if (Master.mode == Mode.None)
            {
                if (Master.Set.DefaultWeather == true) //TODO:天气模块位置判断挪到对应地方, 你可以新建一个方法叫 UpdateWeatherPosition() 来更新天气模块位置
                {
                    WeatherControl.Visibility = Visibility.Visible;
                    if (Master.Set.WeatherPosition == true)
                    {
                        Grid.SetColumn(WeatherControl, 1);
                        TTimes.Visibility = Visibility.Collapsed;
                        TDates.Visibility = Visibility.Visible;
                        TDayofWeek.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        Grid.SetColumn(WeatherControl, 3);
                        TTimes.Visibility = Visibility.Visible;
                        TDates.Visibility = Visibility.Collapsed;
                        TDayofWeek.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    WeatherControl.Visibility = Visibility.Collapsed;
                    TTimes.Visibility = Visibility.Visible;
                    TDates.Visibility = Visibility.Visible;
                    TDayofWeek.Visibility = Visibility.Visible;
                }
                if (Master.Set.Hour24)
                {
                    TTimes.Text = DateTime.Now.ToString("HH:mm");
                }
                else
                {
                    TTimes.Text = DateTime.Now.ToString("hh:mm");
                }
                TDayofWeek.Text = DateTime.Now.ToString("tt dddd");
                TDates.Text = DateTime.Now.ToString("yyyy/MM/dd");
            }
            else
            {
                if (Master.Set.Hour24)
                {
                    TDayofWeek.Text = DateTime.Now.ToString("tt HH:mm");
                }
                else
                {
                    TDayofWeek.Text = DateTime.Now.ToString("tt hh:mm");
                }
            }
        }
        /// <summary>
        /// 倒计时模式UI更新
        /// </summary>
        public void CountDownMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (Master.mode == Mode.CountDown)
            {
                if (IsPause)
                {
                    ContinueCountDown();
                    CountDownMenuItem.Header = "暂停倒计时".Translate();
                    Master.mCountDown.Header = "暂停倒计时".Translate();
                }
                else
                {
                    PauseCountDown();
                    CountDownMenuItem.Header = "继续倒计时".Translate();
                    Master.mCountDown.Header = "继续倒计时".Translate();
                }
            }
            else
            {
                CountDownInput input = new CountDownInput(TimeSpan.FromMinutes(Master.Set.DefaultCountDown));
                if (input.ShowDialog() == true && input.Return != TimeSpan.Zero)
                {
                    StartCountDown(input.Return);
                    CountDownMenuItem.Header = "暂停倒计时".Translate();
                    Master.mCountDown.Header = "暂停倒计时".Translate();
                }
            }
        }
        /// <summary>
        /// 默认状态右键点击响应
        /// </summary>
        private void SettingMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Master.Setting();
        }
        /// <summary>
        /// 计时模式菜单右键点击响应
        /// </summary>
        public void TimingMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (Master.mode == Mode.Timing)
            {
                if (IsPause)
                {
                    ContinueTiming();
                    TimingMenuItem.Header = "暂停计时".Translate();
                    Master.mTiming.Header = "暂停计时".Translate();
                }
                else
                {
                    PauseTiming();
                    TimingMenuItem.Header = "继续计时".Translate();
                    Master.mTiming.Header = "继续计时".Translate();
                }
            }
            else
            {
                StartTiming();
                TimingMenuItem.Header = "暂停计时".Translate();
                Master.mTiming.Header = "暂停计时".Translate();
            }
        }
        /// <summary>
        /// 工作模式菜单右键点击响应
        /// </summary>
        public void WorkMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (Master.mode == Mode.Tomato_Work)
            {
                if (MessageBoxX.Show("是否停止当前工作?".Translate(), "停止工作".Translate(), MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    Master.mode = Mode.None;
                    CountTimer.IsEnabled = false;
                    TOthers.Visibility = Visibility.Collapsed;
                    PBTimeLeft.Visibility = Visibility.Collapsed;
                    WorkMenuItem.Header = "开始工作".Translate();
                    Master.mTotmatoWork.Header = "开始工作".Translate();
                }
            }
            else if (Master.mode == Mode.Tomato_Rest)
            {
                if (MessageBoxX.Show("是否停止当前休息?".Translate(), "停止休息".Translate(), MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    Master.mode = Mode.None;
                    CountTimer.IsEnabled = false;
                    TOthers.Visibility = Visibility.Collapsed;
                    PBTimeLeft.Visibility = Visibility.Collapsed;
                    WorkMenuItem.Header = "开始工作".Translate();
                    Master.mTotmatoWork.Header = "开始工作".Translate();
                }
            }
            else
            {
                StartWork();
                WorkMenuItem.Header = "停止工作".Translate();
                Master.mTotmatoWork.Header = "停止工作".Translate();
            }
        }
        /// <summary>
        /// 长休息菜单右键点击响应
        /// </summary>
        public void RestMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (Master.mode == Mode.Tomato_Rest || Master.mode == Mode.Tomato_Rest_Long)
            {
                if (MessageBoxX.Show("是否停止当前休息?\n扣除的番茄不会被退还".Translate(), "停止休息".Translate(), MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    Master.mode = Mode.None;
                    CountTimer.IsEnabled = false;
                    TOthers.Visibility = Visibility.Collapsed;
                    PBTimeLeft.Visibility = Visibility.Collapsed;
                    WorkMenuItem.Header = "开始休息".Translate();
                    Master.mTotmatoWork.Header = "开始休息".Translate();
                }
            }
            else
            {
                int need = (int)Math.Round(Master.Set.Tomato_RestTimeLong / 2);
                if (Master.Set.Tomato_Count <= Master.Set.Tomato_RestTimeLong / 2 && MessageBoxResult.Yes ==
                    MessageBoxX.Show("是否开始休息?\n休息所需番茄 {0}\n当前拥有番茄 {1}".Translate(need, Master.Set.Tomato_Count),
                    "开始休息".Translate(), MessageBoxButton.YesNo))
                {
                    Master.Set.Tomato_Count -= need;
                    StartRestLong();
                    WorkMenuItem.Header = "停止休息";
                    Master.mTotmatoWork.Header = "停止休息";
                }
                else
                {
                    MessageBoxX.Show("当前番茄不足,不能开始长休息\n休息所需番茄 {0}\n当前拥有番茄 {1}".Translate(need, Master.Set.Tomato_Count),
                        "休息失败,请好好工作".Translate());
                }
            }
        }
        /// <summary>
        /// 左键点击响应函数
        /// </summary>
        private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (TDates.Text == "计时结束".Translate())
            {
                Master.mode = Mode.None;
                CountTimer.IsEnabled = false;
                TOthers.Visibility = Visibility.Collapsed;
                PBTimeLeft.Visibility = Visibility.Collapsed;
                CountDownMenuItem.Header = "开始倒计时";
                Master.mCountDown.Header = "开始倒计时";
            }
            else if (TDates.Text == "工作结束".Translate())
                StartRest();
            else if (TDates.Text == "休息结束".Translate() || TDates.Text == "长休息结束".Translate())
                StartWork();
        }

        private void TimeClock_MouseEnter(object sender, MouseEventArgs e)
        {
        }

        private void TimeClock_MouseLeave(object sender, MouseEventArgs e)
        {

        }
    }
}
