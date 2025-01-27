using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace VPet.Plugin.DemoClock
{
    public partial class WeatherPage : Window, INotifyPropertyChanged
    {
        private string _city;
        public string City
        {
            get => _city;
            set
            {
                if (_city != value)
                {
                    _city = value;
                    OnPropertyChanged(nameof(City));
                }
            }
        }

        private List<WeatherCardViewModel> _weatherDataList;
        public List<WeatherCardViewModel> WeatherDataList
        {
            get => _weatherDataList;
            set
            {
                if (_weatherDataList != value)
                {
                    _weatherDataList = value;
                    OnPropertyChanged(nameof(WeatherDataList));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public WeatherPage(DemoClock Master)
        {
            InitializeComponent();
            City = "";
            WeatherDataList = new();
            LoadData(Master.weather);
            DataContext = this;
        }

        public bool LoadData(WeatherResponse weatherResponse)
        {
            try
            {
                WeatherDataList = new List<WeatherCardViewModel>();
                foreach (var weatherres in weatherResponse.Forecasts)
                {
                    foreach(var weather in weatherres.Casts)
                    {
                        WeatherDataList.Add(new WeatherCardViewModel
                        {
                            Date = weather.Date.ToShortDateString(),
                            DayWeatherIcon = WeatherIconMapping.GetWeatherIcon(weather.DayWeather.ToString()),
                            DayWeather = weather.DayWeather.ToString(),
                            DayTemperature = weather.DayTempFloat.ToString("F0") + "℃",
                            DayWind = weather.DayWind.ToString() + "风 " + weather.DayPower + "级",
                            NightWeatherIcon = WeatherIconMapping.GetWeatherIcon(weather.NightWeather.ToString()),
                            NightWeather = weather.NightWeather.ToString(),
                            NightTemperature = weather.NightTempFloat.ToString("F0") + "℃",
                            NightWind = weather.NightWind.ToString() + "风 " + weather.NightPower + "级",
                        });
                    }
                }
                CityName.Text = weatherResponse.Forecasts.First().City;
                
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
