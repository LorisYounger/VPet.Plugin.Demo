using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace VPet.Plugin.VPetTTS.Core.Providers
{
    /// <summary>
    /// URL TTS 实现
    /// </summary>
    public class URLTTSCore : TTSCoreBase
    {
        public override string Name => "URL";

        public URLTTSCore(Setting settings) : base(settings)
        {
        }

        public override async Task<byte[]> GenerateAudioAsync(string text)
        {
            try
            {
                if (Settings?.URL == null || string.IsNullOrWhiteSpace(Settings.URL.BaseUrl))
                {
                    OnAudioGenerationError("URL TTS BaseUrl 未配置");
                    return Array.Empty<byte>();
                }

                LogMessage($"TTS (URL): 发送请求，文本长度: {text.Length}");

                var encodedText = HttpUtility.UrlEncode(text);
                var url = Settings.URL.BaseUrl;

                // 替换URL中的占位符
                url = url.Replace("{text}", encodedText);
                url = url.Replace("{voice}", Settings.URL.Voice);

                using var client = CreateHttpClient();
                HttpResponseMessage response;

                if (Settings.URL.Method.ToUpper() == "POST")
                {
                    var content = new StringContent($"text={encodedText}&voice={Settings.URL.Voice}");
                    response = await client.PostAsync(url, content);
                }
                else
                {
                    // GET方法
                    if (!url.Contains("?"))
                        url += "?";
                    else
                        url += "&";
                    
                    url += $"text={encodedText}&voice={Settings.URL.Voice}";
                    response = await client.GetAsync(url);
                }

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    LogMessage($"TTS (URL): API 错误: {response.StatusCode} - {errorContent}");
                    OnAudioGenerationError($"URL TTS 错误: {response.StatusCode}");
                    return Array.Empty<byte>();
                }

                var audioData = await response.Content.ReadAsByteArrayAsync();
                LogMessage($"TTS (URL): 音频生成成功，大小: {audioData.Length} bytes");

                OnAudioGenerated(audioData);
                return audioData;
            }
            catch (Exception ex)
            {
                LogMessage($"TTS (URL): 生成音频异常: {ex.Message}");
                OnAudioGenerationError($"URL TTS 异常: {ex.Message}");
                return Array.Empty<byte>();
            }
        }

        public override string GetAudioFormat()
        {
            return "mp3"; // 默认格式
        }

        protected override void LogMessage(string message)
        {
            Console.WriteLine($"[URLTTSCore] {DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}");
        }
    }
}