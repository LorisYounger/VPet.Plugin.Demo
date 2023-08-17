using LinePutScript;
using LinePutScript.Localization.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.ModMaker
{
    public class ModMaker : MainPlugin
    {
        public ModMaker(IMainWindow mainwin)
            : base(mainwin) { }

        public ILine Set;

        public override void LoadPlugin()
        {
            Set = MW.Set.FindLine("ModMaker");
            MenuItem modset = MW.Main.ToolBar.MenuMODConfig;
            modset.Visibility = Visibility.Visible;
            var menuset = new MenuItem()
            {
                Header = "Mod制作器".Translate(),
                HorizontalContentAlignment = HorizontalAlignment.Center,
            };
            menuset.Click += (s, e) =>
            {
                Setting();
            };
            modset.Items.Add(menuset);
        }

        public override string PluginName => "ModMaker";
        public ModMakerWindow Maker;

        public override void Setting()
        {
            if (Maker == null)
            {
                Maker = new ModMakerWindow();
                Maker.ModMaker = this;
                Maker.Show();
            }
            else
            {
                Maker.Topmost = true;
            }
        }
    }
}
