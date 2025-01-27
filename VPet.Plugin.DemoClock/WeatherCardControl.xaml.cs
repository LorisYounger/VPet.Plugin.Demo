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

namespace VPet.Plugin.DemoClock
{
    /// <summary>
    /// WeatherCardControl.xaml 的交互逻辑
    /// </summary>
    public partial class WeatherCardControl : UserControl
    {
        public WeatherCardControl()
        {
            InitializeComponent();
        }
    }

    public class WeatherCardViewModel
    {
        public string Date { get; set; }
        public string DayWeatherIcon { get; set; }
        public string DayWeather { get; set; }
        public string DayTemperature { get; set; }
        public string DayWind { get; set; }

        public string NightWeatherIcon { get; set; }
        public string NightWeather { get; set; }
        public string NightTemperature { get; set; }
        public string NightWind { get; set; }
    }

}
