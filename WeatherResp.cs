using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using static VPet.Plugin.DemoClock.Forecast.WeatherCast;

namespace VPet.Plugin.DemoClock
{
    /// <summary>
    /// 表示地点响应的模型类
    /// </summary>
    public class LocationResponse
    {
        /// <summary>
        /// 响应状态码，通常为整数
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 响应信息，描述请求的结果
        /// </summary>
        public string Info { get; set; } = string.Empty;

        /// <summary>
        /// 信息代码，通常为整数，用于进一步的状态指示
        /// </summary>
        public int Infocode { get; set; }

        /// <summary>
        /// 省份名称
        /// </summary>
        public string Province { get; set; } = string.Empty;

        /// <summary>
        /// 城市名称
        /// </summary>
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// 区域代码，通常为整数
        /// </summary>
        public int Adcode { get; set; }

        /// <summary>
        /// 矩形区域的坐标，格式为 "西南角经度,西南角纬度;东北角经度,东北角纬度"
        /// </summary>
        public string Rectangle { get; set; } = string.Empty;
    }
    /// <summary>
    /// 表示天气预报的响应模型类
    /// </summary>
    public class WeatherResponse
    {
        /// <summary>
        /// 响应状态码
        /// </summary>
        [JsonProperty("status")]
        public int Status { get; set; }

        /// <summary>
        /// 预报数量
        /// </summary>
        [JsonProperty("count")]
        public int Count { get; set; }

        /// <summary>
        /// 响应信息
        /// </summary>
        [JsonProperty("info")]
        public string Info { get; set; } = string.Empty;

        /// <summary>
        /// 信息代码
        /// </summary>
        [JsonProperty("infocode")] public int Infocode { get; set; }

        /// <summary>
        /// 预报列表
        /// </summary>
        [JsonProperty("forecasts")]
       public List<Forecast> Forecasts { get; set; } = new List<Forecast>();

        /// <summary>
        /// 当前天气信息列表
        /// </summary>
        [JsonProperty("lives")] 
        public List<CurrentWeather> Lives { get; set; } = new List<CurrentWeather>();
    }

    /// <summary>
    /// 表示单个天气预报的模型类
    /// </summary>
    public class Forecast
    {
        /// <summary>
        /// 城市名称
        /// </summary>
        [JsonProperty("city")]
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// 区域代码
        /// </summary>
        [JsonProperty("adcode")]
        public int Adcode { get; set; }

        /// <summary>
        /// 省份名称
        /// </summary>
        [JsonProperty("province")]
        public string Province { get; set; } = string.Empty;

        /// <summary>
        /// 报告时间
        /// </summary>
        [JsonProperty("reporttime")]
        public DateTime ReportTime { get; set; }

        /// <summary>
        /// 天气预报列表
        /// </summary>
        [JsonProperty("casts")]
   
        public List<WeatherCast> Casts { get; set; } = new List<WeatherCast>();

        /// <summary>
        /// 表示单个天气情况的模型类
        /// </summary>
        public class WeatherCast
        {
            /// <summary>
            /// 日期
            /// </summary>
            [JsonProperty("date")]
            public DateTime Date { get; set; }

            /// <summary>
            /// 白天天气情况
            /// </summary>
            [JsonProperty("dayweather")]
            public WeatherCondition DayWeather { get; set; }

            /// <summary>
            /// 晚上天气情况
            /// </summary>
            [JsonProperty("nightweather")]
            public WeatherCondition NightWeather { get; set; }

            /// <summary>
            /// 白天风向
            /// </summary>
            [JsonProperty("daywind")]
            public WindDirection DayWind { get; set; }

            /// <summary>
            /// 晚上风向
            /// </summary>
            [JsonProperty("nightwind")]
            public WindDirection NightWind { get; set; }

            /// <summary>
            /// 白天风力
            /// </summary>
            [JsonProperty("daypower")]
            public string DayPower { get; set; } = string.Empty;

            /// <summary>
            /// 晚上风力
            /// </summary>
            [JsonProperty("nightpower")]
            public string NightPower { get; set; } = string.Empty;

            /// <summary>
            /// 白天温度（浮动）
            /// </summary>
            [JsonProperty("daytemp_float")]
            public double DayTempFloat { get; set; }

            /// <summary>
            /// 晚上温度（浮动）
            /// </summary>
            [JsonProperty("nighttemp_float")]
            public double NightTempFloat { get; set; }

            /// <summary>
            /// 天气现象枚举
            /// </summary>
            public enum WeatherCondition
            {
                晴, 少云, 晴间多云, 多云, 阴, 有风, 平静, 微风, 和风, 清风,
                强风, 疾风, 大风, 烈风, 风暴, 狂爆风, 飓风, 热带风暴, 霾,
                中度霾, 重度霾, 严重霾, 阵雨, 雷阵雨, 雷阵雨并伴有冰雹,
                小雨, 中雨, 大雨, 暴雨, 大暴雨, 特大暴雨, 强阵雨, 强雷阵雨,
                极端降雨, 毛毛雨, 雨, 小雨中雨, 中雨大雨, 大雨暴雨,
                暴雨大暴雨, 大暴雨特大暴雨, 雨雪天气, 雨夹雪, 阵雨夹雪,
                冻雨, 雪, 阵雪, 小雪, 中雪, 大雪, 暴雪, 小雪中雪,
                中雪大雪, 大雪暴雪, 浮尘, 扬沙, 沙尘暴, 强沙尘暴,
                龙卷风, 雾, 浓雾, 强浓雾, 轻雾, 大雾, 特强浓雾, 热, 冷, 未知
            }

            /// <summary>
            /// 风向枚举
            /// </summary>
            public enum WindDirection
            {
                无风向, 东北, 东, 东南, 南, 西南, 西, 西北, 北, 旋转不定
            }
        }
    }


    /// <summary>
    /// 表示单个城市当前天气的模型类
    /// </summary>
    public class CurrentWeather
    {
        /// <summary>
        /// 省份名称
        /// </summary>
        [JsonProperty("province")]
        public string Province { get; set; } = string.Empty;

        /// <summary>
        /// 城市名称
        /// </summary>
        [JsonProperty("city")]
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// 区域代码
        /// </summary>
        [JsonProperty("adcode")]
        public int Adcode { get; set; }

        /// <summary>
        /// 当前天气情况
        /// </summary>
        [JsonProperty("weather")]
        public WeatherCondition Weather { get; set; }


        /// <summary>
        /// 风向
        /// </summary>
        [JsonProperty("winddirection")]
        public WindDirection WindDirection { get; set; }

        /// <summary>
        /// 风力
        /// </summary>
        [JsonProperty("windpower")]
        public string WindPower { get; set; } = string.Empty;

        /// <summary>
        /// 报告时间
        /// </summary>
        [JsonProperty("reporttime")]
        public DateTime ReportTime { get; set; }

        /// <summary>
        /// 温度（浮动）
        /// </summary>
        [JsonProperty("temperature_float")]
        public double TemperatureFloat { get; set; }

        /// <summary>
        /// 湿度（浮动）
        /// </summary>
        [JsonProperty("humidity_float")]
        public double HumidityFloat { get; set; }
    }


}
