using LinePutScript.Converter;
using System;
using System.Collections.Generic;
using System.IO;
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
            //TODO VolumeSilder
            PitchSilder.Value = vts.Set.Pitch;
            RateSilder.Value = vts.Set.Rate;
            CombSpeaker.Text = vts.Set.Speaker;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            vts.Set.Enable = SwitchOn.IsChecked.Value;
            vts.Set.Pitch = PitchSilder.Value;
            vts.Set.Rate = RateSilder.Value;
            vts.Set.Speaker = CombSpeaker.Text;
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
    }
}
