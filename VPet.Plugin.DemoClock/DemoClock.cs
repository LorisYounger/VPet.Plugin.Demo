using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Controls;
using System.Windows;
using VPet_Simulator.Windows.Interface;
using System.Windows.Threading;
using LinePutScript.Localization.WPF;
using LinePutScript.Converter;
using LinePutScript;
using Panuon.WPF.UI;

namespace VPet.Plugin.DemoClock
{
    /// <summary>
    /// Demo 时钟
    /// </summary>
    public class DemoClock : MainPlugin
    {

        public enum Mode
        {
            /// <summary>
            /// 正常显示时间
            /// </summary>
            None,
            /// <summary>
            /// 倒计时
            /// </summary>
            CountDown,
            /// <summary>
            /// 正计时
            /// </summary>
            Timing,
            /// <summary>
            /// 番茄钟:工作
            /// </summary>
            Tomato_Work,
            /// <summary>
            /// 番茄钟:休息
            /// </summary>
            Tomato_Rest,
            /// <summary>
            /// 番茄钟:长休息
            /// </summary>
            Tomato_Rest_Long,
            /// <summary>
            /// 时间到 暂停状态
            /// </summary>
            CountDown_End
        }
        /// <summary>
        /// 当前时钟模式
        /// </summary>
        public Mode mode = Mode.None;
        MenuItem menuItem;
        public TimeClock WPFTimeClock;
        public Setting Set;

        public MenuItem mTotmatoWork;
        public MenuItem mTotmatoRest;
        public MenuItem mCountDown;
        public MenuItem mTiming;
        public MenuItem mWeather;

        public long CountDownLength;
        public winSetting winSetting;
        public MusicPlayer musicPlayer;
        public WeatherPage weatherWindow;
        public ZoneInput Zoneinput;
        public WeatherResponse weather; 
        public DemoClock(IMainWindow mainwin) : base(mainwin)
        {
        }

        public override void LoadPlugin()
        {
            Set = new Setting(MW.Set["DemoClock"]);
            MW.Set["DemoClock"] = Set;
            if (Set == null)
            {
                MessageBox.Show("Error");
            }
            WPFTimeClock = new TimeClock(this);
            musicPlayer = new MusicPlayer();
            menuItem = new MenuItem()
            {
                Header = "DM时钟".Translate(),
                HorizontalContentAlignment = HorizontalAlignment.Center,
                Visibility = Visibility.Visible
            };
            //foreach (MenuItem mi in WPFTimeClock.CM.Items)
            //    menuItem.Items.Add(mi);

            var mi = new MenuItem()
            {
                Header = "设置".Translate(),
                HorizontalContentAlignment = HorizontalAlignment.Center,
            };
            mi.Click += (s, e) => { Setting(); };
            menuItem.Items.Add(mi);
            menuItem.Items.Add(new Separator());
            mCountDown = new MenuItem()
            {
                Header = "开始倒计时".Translate(),
                HorizontalContentAlignment = HorizontalAlignment.Center,
            };
            mCountDown.Click += WPFTimeClock.CountDownMenuItem_Click;
            menuItem.Items.Add(mCountDown);

            mTiming = new MenuItem()
            {
                Header = "开始正计时".Translate(),
                HorizontalContentAlignment = HorizontalAlignment.Center,
            };
            mTiming.Click += WPFTimeClock.TimingMenuItem_Click;
            menuItem.Items.Add(mTiming);

            mTotmatoWork = new MenuItem()
            {
                Header = "开始工作".Translate(),
                HorizontalContentAlignment = HorizontalAlignment.Center,
            };
            mTotmatoWork.Click += WPFTimeClock.WorkMenuItem_Click;
            menuItem.Items.Add(mTotmatoWork);

            mTotmatoRest = new MenuItem()
            {
                Header = "开始休息".Translate(),
                HorizontalContentAlignment = HorizontalAlignment.Center,
            };
            mTotmatoRest.Click += WPFTimeClock.RestMenuItem_Click;
            menuItem.Items.Add(mTotmatoRest);

            MenuItem modset = MW.Main.ToolBar.MenuMODConfig;
            modset.Visibility = Visibility.Visible;
            var menuset = new MenuItem()
            {
                Header = "DM时钟".Translate(),
                HorizontalContentAlignment = HorizontalAlignment.Center,
            };
            menuset.Click += (s, e) => { Setting(); };
            modset.Items.Add(menuset);

            var menuweather = new MenuItem()
            {
                Header = "天气".Translate(),
                HorizontalContentAlignment = HorizontalAlignment.Center,
            };
            var menuweatherpage = new MenuItem()
            {
                Header = "天气页面".Translate(),
                HorizontalContentAlignment = HorizontalAlignment.Center,
            };
            menuweatherpage.Click += (s, e) => { weatherWindow = new WeatherPage(this); weatherWindow.Show(); };
            var menuregionset = new MenuItem()
            {
                Header = "地区设置".Translate(),
                HorizontalContentAlignment = HorizontalAlignment.Center,
            };
            menuregionset.Click += (s, e) => { Zoneinput = new ZoneInput(this); Zoneinput.Show(); };
            var weatherset = new MenuItem()
            {
                Header = "UI设置".Translate(),
                HorizontalContentAlignment = HorizontalAlignment.Center,
            };
            weatherset.Click += (s, e) => { WeatherSetting(); };
            menuweather.Items.Add(menuregionset);
            menuweather.Items.Add(weatherset);

            mWeather = new MenuItem()
            {
                Header = "天气".Translate(),
                HorizontalContentAlignment = HorizontalAlignment.Center,
                Visibility = Visibility.Visible
            };
            mWeather.Items.Add(menuweatherpage);

            modset.Items.Add(menuweather);
            ///***************** 设置天气 *****************///
            HandleWeatherAsync();
            Tools.StartRecurringTimer(3, HandleWeatherAsync);
            //WPFTimeClock.WeatherControl.SetWeather();
            ///***************** 设置天气 *****************///
        }
        public override void LoadDIY()
        {
            MW.Main.ToolBar.MenuDIY.Items.Add(menuItem);
            MW.Main.ToolBar.MenuDIY.Items.Add(mWeather);
        }

        internal async Task HandleWeatherAsync()
        {
            // 获取天气信息
            weather = await GetWeatherAsync("https://weather.exlb.net/Weather");

            if (weather.Status != 200)
            {
                if (Set.AdCode == 0)
                {
                    // 如果没有 AdCode，弹出输入框
                    Zoneinput = new ZoneInput(this);
                    MW.Dispatcher.Invoke(() =>
                    {
                        Zoneinput.Show();
                    });
                }
                else
                {
                    // 如果有 AdCode，使用 AdCode 请求天气
                    weather = await GetWeatherAsync("https://weather.exlb.net/Weather", $"adcode={Set.AdCode}");
                }
            }
            if(weather.Status.Equals(200))
            {
                WPFTimeClock.WeatherControl.SetWeather(weather.Lives.Last().City, "温度:" + weather.Lives.Last().TemperatureFloat.ToString("F0") + "℃"
                    , weather.Lives.Last().Weather.ToString(), weather.Lives.Last().WindDirection.ToString() + "风 " + weather.Lives.Last().WindPower + "级"
                    , "湿度:" + weather.Lives.Last().HumidityFloat.ToString("F0") + "%");
            }
        }

        private async Task<WeatherResponse> GetWeatherAsync(string url,string param = "", int timeoutSeconds = 5)
        {
            try
            {
                // 创建一个带有超时的 Http 请求任务
                using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                string response = await HttpHelper.SendGetRequestAsync(url, param, cts.Token);

                // 解析返回的天气数据
                var lps = new LPS(response);
                return LPSConvert.DeserializeObject<WeatherResponse>(lps, convertNoneLineAttribute: true);
            }
            catch (TaskCanceledException)
            {
                MessageBox.Show("请求超时，请稍后再试。");
                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"请求失败：{ex.Message}");
                return null;
            }
        }

        public override string PluginName => "DemoClock";

        public override void Setting()
        {
            if (winSetting == null)
            {
                winSetting = new winSetting(this);
                winSetting.Show();
            }
            else
            {
                winSetting.Topmost = true;
            }
        }
        public void WeatherSetting()
        {
            if(winSetting == null)
            {
                winSetting = new winSetting(this);
                winSetting.SetControl.SelectedIndex = 2;
                winSetting.Show();
            }
            else
            {
                winSetting.Close();
                winSetting = new winSetting(this);
                winSetting.SetControl.SelectedIndex = 2;
                winSetting.Show();
            }
        }
    }
}
