using Panuon.WPF.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VPet.Plugin.DemoClock
{ 
    /// <summary>
    /// Weather.xaml 的交互逻辑
    /// </summary>
    public partial class Weather : System.Windows.Controls.UserControl
    {
        public event Action OnWeatherPageShow;
        public Weather()
        {
            InitializeComponent();
            Description = "大暴雨特大暴雨";
            WeatherImage = WeatherIconMapping.GetWeatherIcon(Description);
            DataContext = this;
        }

        // 城市
        public string City
        {
            get => CityText.Text;
            set => CityText.Text = value;
        }

        // 温度
        public string Temperature
        {
            get => TemperatureText.Text;
            set => TemperatureText.Text = value;
        }

        // 天气图标
        public string WeatherImage
        {
            get => WeatherIcon.Text;
            set => WeatherIcon.Text = value;
        }

        public string Description
        {
            get => WeatherIcon.ToolTip.ToString();
            set => WeatherIcon.ToolTip = value;
        }

        public string Wind
        {
            get => WindText.Text;
            set => WindText.Text = value;
        }

        public string Humidity
        {
            get => HumidityText.Text;
            set => HumidityText.Text = value;
        }

        public bool SetWeather(string city, string temperature, string description, string wind, string humidity)
        {
            try
            {
                City = city;
                OnPropertyChanged(nameof(City));
                Temperature = temperature;
                OnPropertyChanged(nameof(Temperature));
                Description = description;
                OnPropertyChanged(nameof(Description));
                WeatherImage = WeatherIconMapping.GetWeatherIcon(description);
                OnPropertyChanged(nameof(WeatherImage));
                Wind = wind;
                OnPropertyChanged(nameof(Wind));
                Humidity = humidity;
                OnPropertyChanged(nameof(Humidity));
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"设置天气信息时发生错误: {ex.Message}");
                return false;
            }

        }

        internal void UserControl_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OnWeatherPageShow?.Invoke();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
