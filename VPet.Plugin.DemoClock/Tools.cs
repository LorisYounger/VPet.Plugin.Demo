using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VPet.Plugin.DemoClock
{
    internal static class Tools
    {
        /// <summary>
        /// 语音类型检测函数
        /// </summary>
        /// <param name="input">原始语音内容</param>
        /// <param name="value">去掉类型符后的语音内容</param>
        /// <returns>语音类型</returns>
        public static bool TryGetInputTypeAndValue(string input, out string value)
        {
            value = string.Empty;

            if (string.IsNullOrEmpty(input))
            {
                return false;
            }

            // 检查 "file:" 前缀
            if (input.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
            {
                value = input.Substring(5); // 去掉 "file:"
                return true;
            }
            // 检查 "text:" 前缀
            else if (input.StartsWith("text:", StringComparison.OrdinalIgnoreCase))
            {
                value = input.Substring(5); // 去掉 "text:"
                return false;
            }
            return false;
        }
        /// <summary>
        /// 标志位替换函数
        /// </summary>
        /// <param name="input">原始文本，包含标志位(标志位格式为{标志位})(</param>
        /// <param name="replacements">替换字典</param>
        /// <returns>替换后文本</returns>
        public static string ReplacePlaceholders(string input, Dictionary<string, string> replacements)
        {
            if (string.IsNullOrEmpty(input) || replacements == null || replacements.Count == 0)
            {
                return input; // 如果输入为空或没有替换规则，返回原文本
            }

            // 使用正则表达式查找所有的标志符（格式为 {标志符}）
            string pattern = @"\{([^\}]+)\}";
            return Regex.Replace(input, pattern, match =>
            {
                string placeholder = match.Groups[1].Value; // 提取标志符名（去掉 {}）
                return replacements.ContainsKey(placeholder) ? replacements[placeholder] : match.Value; // 如果有替换值则替换，否则保持原样
            });
        }
        /// <summary>
        /// 加载嵌入的资源文件
        /// </summary>
        /// <param name="resourceName">资源文件的名称（包括命名空间）</param>
        /// <returns>返回资源文件内容</returns>
        public static string LoadEmbeddedResource(string Namespace, string resourceName)
        {
            try
            {
                // 获取当前执行的程序集
                var assembly = Assembly.GetExecutingAssembly();

                // 构建资源文件的名称
                // 注意：嵌入的资源文件的命名通常为：命名空间+文件夹+文件名（以点分隔）
                var resourcePath = $"{Namespace}.Resources.{resourceName}";

                // 打开资源流
                using (var stream = assembly.GetManifestResourceStream(resourcePath))
                {
                    if (stream == null)
                    {
                        throw new FileNotFoundException($"Resource {resourcePath} not found.");
                    }

                    // 读取资源内容
                    using (var reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();  // 返回文件的内容
                    }
                }
            }
            catch (Exception ex)
            {
                // 错误处理
                return $"Error: {ex.Message}";
            }
        }

        public static void StartRecurringTimer(double intervalInHours, Func<Task> asyncActionToExecute)
        {
            // 检查参数有效性
            if (intervalInHours <= 0)
                throw new ArgumentOutOfRangeException(nameof(intervalInHours), "间隔时间必须大于 0 小时。");
            if (asyncActionToExecute == null)
                throw new ArgumentNullException(nameof(asyncActionToExecute), "执行操作不能为空。");

            // 启动后台任务以确保不阻塞主线程
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        // 执行异步操作
                        await asyncActionToExecute();

                        // 计算延迟时间（以毫秒为单位），并避免超出数据类型的限制
                        TimeSpan interval = TimeSpan.FromHours(intervalInHours);
                        if (interval.TotalMilliseconds > int.MaxValue)
                        {
                            // 如果时间过长，分段延迟以防止溢出
                            int chunkMilliseconds = int.MaxValue;
                            double remainingMilliseconds = interval.TotalMilliseconds;

                            while (remainingMilliseconds > 0)
                            {
                                int delay = (int)Math.Min(chunkMilliseconds, remainingMilliseconds);
                                await Task.Delay(delay);
                                remainingMilliseconds -= delay;
                            }
                        }
                        else
                        {
                            // 普通延迟
                            await Task.Delay(interval);
                        }
                    }
                    catch (Exception ex)
                    {
                        // 处理异常并记录日志（根据需要）
                        Console.WriteLine($"定时器执行时发生异常：{ex.Message}");
                    }
                }
            });
        }

    }

    public static class HttpHelper
    {
        private static readonly HttpClient client = new HttpClient();

        /// <summary>
        /// 发送带单一参数的 GET 请求
        /// </summary>
        /// <param name="url">请求的 URL</param>
        /// <param name="parameter">请求的单一参数（例如: param=value）</param>
        /// <returns>返回的响应内容</returns>
        public static async Task<string> SendGetRequestAsync(string url, string parameter, System.Threading.CancellationToken cancellationToken)
        {
            try
            {
                var fullUrl = "";
                if (string.IsNullOrWhiteSpace(parameter))
                    fullUrl = url;
                else
                    fullUrl = $"{url}?{parameter}";

                // 发送 GET 请求
                HttpResponseMessage response = await client.GetAsync(fullUrl, cancellationToken);

                // 检查响应状态
                if (response.IsSuccessStatusCode)
                {
                    // 获取响应内容
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    throw new Exception($"Request failed with status code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                // 错误处理
                return $"Error: {ex.Message}";
            }
        }
    }

    public static class WeatherIconMapping
    {
        // 定义一个字典，将天气描述和对应的Unicode编码映射
        public static readonly Dictionary<string, string> WeatherIcons = new Dictionary<string, string>
    {
        { "晴", "\uF1BF" },
        { "少云", "\uF1BB" },
        { "晴间多云", "\uF1BB" },
        { "多云", "\uEBA4" },
        { "阴", "\uEBA4" },
        { "有风", "\uF2C9" },
        { "平静", "\uE6BF" },
        { "微风", "\uF2C9" },
        { "和风", "\uF2C9" },
        { "清风", "\uF2C9" },
        { "强风", "\uF2C9" },
        { "疾风", "\uF2C9" },
        { "大风", "\uF2C9" },
        { "烈风", "\uF2C9" },
        { "风暴", "\uF2C9" },
        { "狂爆风", "\uF2C9" },
        { "飓风", "\uF23D" },
        { "热带风暴", "\uF23D" },
        { "霾", "\uEE00" },
        { "中度霾", "\uEDFE" },
        { "重度霾", "\uEDFE" },
        { "严重霾", "\uEDFE" },
        { "阵雨", "\uF122" },
        { "雷阵雨", "\uF209" },
        { "雷阵雨并伴有冰雹", "\uEDED" },
        { "小雨", "\uF056" },
        { "中雨", "\uEC68" },
        { "大雨", "\uF122" },
        { "暴雨", "\uEE15" },
        { "大暴雨", "\uEE15" },
        { "特大暴雨", "\uEE15" },
        { "强阵雨", "\uEE15" },
        { "强雷阵雨", "\uF209" },
        { "极端降雨", "\uEE14" },
        { "毛毛雨", "\uF056" },
        { "雨", "\uF056" },
        { "小雨中雨", "\uEC68" },
        { "中雨大雨", "\uF122" },
        { "大雨暴雨", "\uEE15" },
        { "暴雨大暴雨", "\uEE15" },
        { "大暴雨特大暴雨", "\uEE15" },
        { "雨雪天气", "\uF15E" },
        { "雨夹雪", "\uF15E" },
        { "阵雨夹雪", "\uF15E" },
        { "冻雨", "\uF15E" },
        { "雪", "\uF15E" },
        { "阵雪", "\uF15E" },
        { "小雪", "\uF15E" },
        { "中雪", "\uF15E" },
        { "大雪", "\uF15E" },
        { "暴雪", "\uF15E" },
        { "小雪中雪", "\uF15E" },
        { "中雪大雪", "\uF15E" },
        { "大雪暴雪", "\uF15E" },
        { "浮尘", "\uEDFE" },
        { "扬沙", "\uEDFE" },
        { "沙尘暴", "\uEDFE" },
        { "强沙尘暴", "\uEDFE" },
        { "龙卷风", "\uF21D" },
        { "雾", "\uED50" },
        { "浓雾", "\uED50" },
        { "强浓雾", "\uED50" },
        { "轻雾", "\uED50" },
        { "大雾", "\uED50" },
        { "特强浓雾", "\uED50" },
        { "热", "\uF1F3" },
        { "冷", "\uF1F2" },
        { "未知", "\uF046" }
    };

        // 使用示例
        public static string GetWeatherIcon(string weatherDescription)
        {
            if (WeatherIcons.ContainsKey(weatherDescription))
            {
                return WeatherIcons[weatherDescription];
            }
            return WeatherIcons["未知"]; // 默认返回未知图标
        }
    }
}
