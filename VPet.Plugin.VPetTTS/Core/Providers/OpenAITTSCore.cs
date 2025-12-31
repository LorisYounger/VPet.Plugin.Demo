using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace VPet.Plugin.VPetTTS.Core.Providers
{
    /// <summary>
    /// OpenAI TTS 实现
    /// </summary>
    public class OpenAITTSCore : TTSCoreBase
    {
        public override string Name => "OpenAI";

        public OpenAITTSCore(Setting settings) : base(settings)
        {
        }

        public override async Task<byte[]> GenerateAudioAsync(string text)
        {
            try
            {
                if (Settings?.OpenAI == null || string.IsNullOrWhiteSpace(Settings.OpenAI.ApiKey))
                {
                    OnAudioGenerationError("OpenAI API Key 未配置");
                    return Array.Empty<byte>();
                }

                LogMessage($"TTS (OpenAI): 发送请求，文本长度: {text.Length}");

                var requestBody = new
                {
                    model = Settings.OpenAI.Model,
                    input = text,
                    voice = Settings.OpenAI.Voice,
                    response_format = Settings.OpenAI.Format
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var client = CreateHttpClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Settings.OpenAI.ApiKey}");

                var response = await client.PostAsync($"{Settings.OpenAI.BaseUrl}/audio/speech", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    LogMessage($"TTS (OpenAI): API 错误: {response.StatusCode} - {errorContent}");
                    OnAudioGenerationError($"OpenAI TTS 错误: {response.StatusCode}");
                    return Array.Empty<byte>();
                }

                var audioData = await response.Content.ReadAsByteArrayAsync();
                LogMessage($"TTS (OpenAI): 音频生成成功，大小: {audioData.Length} bytes");

                OnAudioGenerated(audioData);
                return audioData;
            }
            catch (Exception ex)
            {
                LogMessage($"TTS (OpenAI): 生成音频异常: {ex.Message}");
                OnAudioGenerationError($"OpenAI TTS 异常: {ex.Message}");
                return Array.Empty<byte>();
            }
        }

        public override string GetAudioFormat()
        {
            return Settings?.OpenAI?.Format ?? "mp3";
        }

        protected override void LogMessage(string message)
        {
            Console.WriteLine($"[OpenAITTSCore] {DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}");
        }
    }
}