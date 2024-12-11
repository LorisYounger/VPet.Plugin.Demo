using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VPet_Simulator.Windows.Interface;
using LinePutScript.Localization.WPF;
using System.Windows.Controls;

namespace VPet.Plugin.Monitor
{

    public class MonitorMain : MainPlugin
    {
        public MonitorBlock MB;
        public MenuItem SetButtonDIY;
        public MenuItem SetButtonMOD;
        public winSetting winSetting;
        public Setting Set;
        public MonitorMain(IMainWindow mainwin) : base(mainwin)
        {
        }
        public override void LoadPlugin()
        {
            Set = new Setting(MW.Set["Monitor"]);
            MW.Set["Monitor"] = Set;
            MB = new MonitorBlock(this);
            SetButtonMOD = new MenuItem()
            {
                Header = "监视器设置".Translate(),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            };
            SetButtonMOD.Click += (s, e) => { Setting(); };
            MW.Main.ToolBar.MenuMODConfig.Visibility = System.Windows.Visibility.Visible;
            MW.Main.ToolBar.MenuMODConfig.Items.Add(SetButtonMOD);
        }        
        public override string PluginName => "Performance Monitor";
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
