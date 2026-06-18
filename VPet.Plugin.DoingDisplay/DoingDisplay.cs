
using LinePutScript.Converter;
using LinePutScript.Localization.WPF;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using VPet_Simulator.Windows.Interface;
using Timer = System.Timers.Timer;

namespace VPet.Plugin.DoingDisplay
{
    public class DoingDisplay : MainPlugin
    {
        public Setting Set;

        public DoingDisplay(IMainWindow mainwin) : base(mainwin)
        {
            Set = new Setting();
        }
        public override void LoadPlugin()
        {
            if (MW.GameSavesData["DoingDisplay"] != null)
            {
                var set = LPSConvert.DeserializeObject<Setting>(MW.GameSavesData["DoingDisplay"], convertNoneLineAttribute: true);
                if (set != null)
                {
                    Set = set;
                }
            }
            MenuItem modset = MW.Main.ToolBar.MenuMODConfig;
            modset.Visibility = Visibility.Visible;
            var menuItem = new MenuItem()
            {
                Header = "DoingDisplay".Translate(),
                HorizontalContentAlignment = HorizontalAlignment.Center,
            };
            menuItem.Click += (s, e) => { Setting(); };
            modset.Items.Add(menuItem);
            timer.Elapsed += Timer_Elapsed;
        }

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            var fgapp = ForegroundAppHelper.GetForegroundAppInfo();
            if (MW.Main.NowWork != null)
            {
                MW.Main.NowWork.nametrans = fgapp.WindowTitle;
            }
            var sts = Set.Statisticals.Find(x => x.SoftWareProcessName == fgapp.ProcessName);
            if (sts == null)
            {
                Set.Statisticals.Add(new Setting.SoftwareStatistical()
                {
                    SoftWareProcessName = fgapp.ProcessName,
                    SoftWare = fgapp.WindowTitle,
                    LastUse = DateTime.Now,
                    Minute = 1
                });
            }
            else
            {
                if (!sts.isEdit)
                {
                    sts.SoftWare = fgapp.WindowTitle;
                }
                sts.LastUse = DateTime.Now;
                sts.Minute++;
            }
            DateTime date = DateTime.Now.Date;
            var dsts = Set.Dailies.Find(x => x.Date == date);
            if (dsts == null)
            {
                Set.Dailies.Add(new Setting.DailyStatistical()
                {
                    Date = date,
                    Minute = 1,
                    SoftwareUsage = new Dictionary<string, int>() { { fgapp.ProcessName, 1 } }
                });
            }
            else
            {
                dsts.Minute++;
                if (dsts.SoftwareUsage.ContainsKey(fgapp.ProcessName))
                {
                    dsts.SoftwareUsage[fgapp.ProcessName]++;
                }
                else
                {
                    dsts.SoftwareUsage[fgapp.ProcessName] = 1;
                }
            }
        }

        Timer timer = new Timer(60000)
        {
            AutoReset = true,
            Enabled = true
        };
        public override string PluginName => "DoingDisplay";

        public winSetting? winSetting;
        public override void Save()
        {
            MW.GameSavesData["DoingDisplay"] = LPSConvert.SerializeObject(Set, "DoingDisplay", convertNoneLineAttribute: true);
        }
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
    }

}
