using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VPet_Simulator.Windows.Interface;
using EdgeTTS;
using LinePutScript.Converter;
using LinePutScript;
using System.IO;
using VPet_Simulator.Core;
using System.Windows.Controls;
using System.Windows;
using LinePutScript.Localization.WPF;

namespace VPet.Plugin.VPetTTS
{
    public class EdgeTTS : MainPlugin
    {
        public EdgeTTSClient etts;
        public Setting Set;
        public EdgeTTS(IMainWindow mainwin) : base(mainwin)
        {
            etts = new EdgeTTSClient();
        }
        public override void LoadPlugin()
        {
            Set = LPSConvert.DeserializeObject<Setting>(MW.Set["EdgeTTS"]);

            if (!Directory.Exists(GraphCore.CachePath + @"\voice"))
                Directory.CreateDirectory(GraphCore.CachePath + @"\voice");
            if (Set.Enable)
                MW.Main.OnSay += Main_OnSay;

            //根据当前语言选择合适的默认发音人
            if (string.IsNullOrEmpty(Set.Speaker))
                if ("EdgeTTSSpeaker".Translate() == "EdgeTTSSpeaker")
                    Set.Speaker = "en-US-AnaNeural";
                else
                    Set.Speaker = "EdgeTTSSpeaker".Translate();


            if (Set.Sec_MS_GEC_URL == " ")
                if (LocalizeCore.CurrentCulture == "zh-Hans")
                    Set.Sec_MS_GEC_URL = "http://123.207.46.66:8086/api/getGec";
                else
                    Set.Sec_MS_GEC_URL = "";
            etts.Sec_MS_GEC_UpDate_Url = Set.Sec_MS_GEC_URL;

            MenuItem modset = MW.Main.ToolBar.MenuMODConfig;
            modset.Visibility = Visibility.Visible;
            var menuItem = new MenuItem()
            {
                Header = "EdgeTTS",
                HorizontalContentAlignment = HorizontalAlignment.Center,
            };
            menuItem.Click += (s, e) => { Setting(); };
            modset.Items.Add(menuItem);
        }
        //public override void LoadDIY()
        //{
        //    MW.Main.ToolBar.AddMenuButton(VPet_Simulator.Core.ToolBar.MenuType.DIY, "EdgeTTS", Setting);
        //}
        public void Main_OnSay(string saythings)
        {//说话语音
            var path = GraphCore.CachePath + $"\\voice\\{Sub.GetHashCode(saythings):X}.mp3";
            if (File.Exists(path))
            {
                MW.Main.PlayVoice(new Uri(path));
            }
            else
            {
                var res = etts.SynthesisAsync(saythings, Set.Speaker, Set.PitchStr, Set.RateStr).Result;
                if (res.Code == ResultCode.Success)
                {
                    FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
                    BinaryWriter w = new BinaryWriter(fs);
                    w.Write(res.Data.ToArray());
                    fs.Close();
                    fs.Dispose();
                    w.Dispose();
                    MW.Main.PlayVoice(new Uri(path));
                }
            }
        }

        public winSetting winSetting;
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
        public override string PluginName => "EdgeTTS";
    }
}
