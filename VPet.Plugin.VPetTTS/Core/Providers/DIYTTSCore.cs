using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace VPet.Plugin.VPetTTS.Core.Providers
{
    /// <summary>
    /// DIY TTS 实现
    /// </summary>
    public class DIYTTSCore : TTSCoreBase
    {
        public override string Name => "DIY";

        public DIYTTSCore(Setting settings) : base(settings)
        {
        }

        public override async Task<byte[]> GenerateAudioAsync(string text)
        {
            try
            {
                if (Settings?.DIY == null || string.IsNullOrWhiteSpace(Settings.DIY.BaseUrl))
                {
                    OnAudioGenerationError("DIY TTS BaseUrl 未配置");
                    return Array.Empty<byte>();
                }

                LogMessage($"TTS (DIY): 发送请求，文本长度: {text.Length}");

                using var client = CreateHttpClient();

                // 添加自定义头
                if (Settings.DIY.CustomHeaders != null)
                {
                    foreach (var header in Settings.DIY.CustomHeaders)
                    {
                        if (header.IsEnabled && !string.IsNullOrWhiteSpace(header.Key))
                        {
                            client.DefaultRequestHeaders.Add(header.Key, header.Value);
                        }
                    }
                }

                HttpResponseMessage response;

                if (Settings.DIY.Method.ToUpper() == "POST")
                {
                    // 替换请求体中的占位符
                    var requestBody = Settings.DIY.RequestBody.Replace("{text}", text);
                    var content = new StringContent(requestBody, Encoding.UTF8, Settings.DIY.ContentType);
                    response = await client.PostAsync(Settings.DIY.BaseUrl, content);
                }
                else
                {
                    // GET方法
                    var url = Settings.DIY.BaseUrl;
                    if (!url.Contains("?"))
                        url += "?";
                    else
                        url += "&";
                    
                    url += $"text={Uri.EscapeDataString(text)}";
                    response = await client.GetAsync(url);
                }

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    LogMessage($"TTS (DIY): API 错误: {response.StatusCode} - {errorContent}");
                    OnAudioGenerationError($"DIY TTS 错误: {response.StatusCode}");
                    return Array.Empty<byte>();
                }

                var audioData = await response.Content.ReadAsByteArrayAsync();
                LogMessage($"TTS (DIY): 音频生成成功，大小: {audioData.Length} bytes");

                OnAudioGenerated(audioData);
                return audioData;
            }
            catch (Exception ex)
            {
                LogMessage($"TTS (DIY): 生成音频异常: {ex.Message}");
                OnAudioGenerationError($"DIY TTS 异常: {ex.Message}");
                return Array.Empty<byte>();
            }
        }

        public override string GetAudioFormat()
        {
            return Settings?.DIY?.ResponseFormat ?? "mp3";
        }

        protected override void LogMessage(string message)
        {
            Console.WriteLine($"[DIYTTSCore] {DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}");
        }
    }
}