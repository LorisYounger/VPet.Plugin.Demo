using EdgeTTS;
using LinePutScript.Converter;
using LinePutScript.Localization.WPF;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using VPet_Simulator.Core;

namespace VPet.Plugin.VPetTTS
{
    /// <summary>
    /// winSetting.xaml 的交互逻辑
    /// </summary>
    public partial class winSetting : Window
    {
        EdgeTTS vts;
        public winSetting(EdgeTTS vts)
        {
            InitializeComponent();
            this.vts = vts;
            SwitchOn.IsChecked = vts.Set.Enable;
            VolumeSilder.Value = vts.MW.Main.PlayVoiceVolume * 100;
            PitchSilder.Value = vts.Set.Pitch;
            RateSilder.Value = vts.Set.Rate;
            CombSpeaker.Text = vts.Set.Speaker;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (vts.Set.Enable != SwitchOn.IsChecked.Value)
            {
                if (SwitchOn.IsChecked.Value)
                    vts.MW.Main.OnSay += vts.Main_OnSay;
                else
                    vts.MW.Main.OnSay -= vts.Main_OnSay;
                vts.Set.Enable = SwitchOn.IsChecked.Value;
            }
            vts.Set.Pitch = PitchSilder.Value;
            vts.Set.Rate = RateSilder.Value;
            vts.Set.Speaker = CombSpeaker.Text;
            vts.MW.Main.PlayVoiceVolume = VolumeSilder.Value / 100;
            vts.MW.Set.Remove("DemoClock");
            vts.MW.Set.Add(LPSConvert.SerializeObject(vts.Set, "DemoClock"));
            foreach (var tmpfile in Directory.GetFiles(GraphCore.CachePath + @"\voice"))
            {
                try
                {
                    File.Delete(tmpfile);
                }
                finally
                {

                }
            }
            Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            vts.winSetting = null;
        }

        private void Test_Click(object sender, RoutedEventArgs e)
        {
            Test.IsEnabled = false;
            var cbt = CombSpeaker.Text;
            var pit = $"{(PitchSilder.Value >= 0 ? "+" : "")}{PitchSilder.Value:f2}Hz";
            var rat = $"{(RateSilder.Value >= 0 ? "+" : "")}{RateSilder.Value:f2}%";
            Task.Run(() =>
            {
                var path = GraphCore.CachePath + $"\\voice\\{DateTime.Now.Ticks:X}.mp3";
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                var res = vts.etts.SynthesisAsync("你好,主人\n现在是".Translate() + DateTime.Now, cbt, pit, rat).Result;
                if (res.Code == ResultCode.Success)
                {
                    FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
                    BinaryWriter w = new BinaryWriter(fs);
                    w.Write(res.Data.ToArray());
                    fs.Close();
                    fs.Dispose();
                    w.Dispose();
                    vts.MW.Main.PlayVoice(new Uri(path));
                }
                else
                {
                    MessageBox.Show("错误代码: {0}\n消息: {1}".Translate(res.Code, res.Message), "生成失败".Translate());
                }
                Dispatcher.Invoke(() => Test.IsEnabled = true);
            });
        }
    }
}
