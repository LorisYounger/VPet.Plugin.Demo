using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace VPet.Plugin.VPetTTS.Core.Providers
{
    /// <summary>
    /// GPT-SoVITS TTS 实现
    /// 支持WebUI和API v2模式
    /// </summary>
    public class GPTSoVITSTTSCore : TTSCoreBase
    {
        public override string Name => "GPT-SoVITS";

        public GPTSoVITSTTSCore(Setting settings) : base(settings)
        {
        }

        public override async Task<byte[]> GenerateAudioAsync(string text)
        {
            try
            {
                if (Settings?.GPTSoVITS == null || string.IsNullOrWhiteSpace(Settings.GPTSoVITS.BaseUrl))
                {
                    OnAudioGenerationError("GPT-SoVITS BaseUrl 未配置");
                    return Array.Empty<byte>();
                }

                LogMessage($"TTS (GPT-SoVITS): 发送请求，文本长度: {text.Length}");

                if (Settings.GPTSoVITS.ApiMode == "ApiV2")
                {
                    return await GenerateAudioApiV2Async(text);
                }
                else
                {
                    return await GenerateAudioWebUIAsync(text);
                }
            }
            catch (Exception ex)
            {
                LogMessage($"TTS (GPT-SoVITS): 生成音频异常: {ex.Message}");
                OnAudioGenerationError($"GPT-SoVITS TTS 异常: {ex.Message}");
                return Array.Empty<byte>();
            }
        }

        private async Task<byte[]> GenerateAudioWebUIAsync(string text)
        {
            var requestBody = new
            {
                text = text,
                text_lang = Settings.GPTSoVITS.TextLanguage,
                ref_audio_path = Settings.GPTSoVITS.ReferWavPath,
                prompt_text = Settings.GPTSoVITS.PromptText,
                prompt_lang = Settings.GPTSoVITS.TextLanguage,
                top_k = 15,
                top_p = 1.0,
                temperature = Settings.GPTSoVITS.Temperature,
                text_split_method = "按标点符号切",
                batch_size = 1,
                speed_factor = Settings.GPTSoVITS.Speed,
                split_bucket = true,
                return_fragment = false,
                fragment_interval = 0.3,
                seed = -1
            };

            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var client = CreateHttpClient();
            var response = await client.PostAsync($"{Settings.GPTSoVITS.BaseUrl}/tts", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                LogMessage($"TTS (GPT-SoVITS WebUI): API 错误: {response.StatusCode} - {errorContent}");
                OnAudioGenerationError($"GPT-SoVITS WebUI 错误: {response.StatusCode}");
                return Array.Empty<byte>();
            }

            var audioData = await response.Content.ReadAsByteArrayAsync();
            LogMessage($"TTS (GPT-SoVITS WebUI): 音频生成成功，大小: {audioData.Length} bytes");

            OnAudioGenerated(audioData);
            return audioData;
        }

        private async Task<byte[]> GenerateAudioApiV2Async(string text)
        {
            var requestBody = new
            {
                text = text,
                text_lang = "zh",
                ref_audio_path = Settings.GPTSoVITS.ReferWavPath,
                prompt_text = Settings.GPTSoVITS.PromptText,
                prompt_lang = "zh",
                top_k = 15,
                top_p = 1.0,
                temperature = Settings.GPTSoVITS.Temperature,
                text_split_method = "cut5",
                batch_size = 1,
                speed_factor = Settings.GPTSoVITS.Speed,
                streaming_mode = 0,
                seed = -1,
                parallel_infer = true,
                repetition_penalty = 1.35
            };

            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var client = CreateHttpClient();
            var response = await client.PostAsync($"{Settings.GPTSoVITS.BaseUrl}/tts", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                LogMessage($"TTS (GPT-SoVITS API v2): API 错误: {response.StatusCode} - {errorContent}");
                OnAudioGenerationError($"GPT-SoVITS API v2 错误: {response.StatusCode}");
                return Array.Empty<byte>();
            }

            var audioData = await response.Content.ReadAsByteArrayAsync();
            LogMessage($"TTS (GPT-SoVITS API v2): 音频生成成功，大小: {audioData.Length} bytes");

            OnAudioGenerated(audioData);
            return audioData;
        }

        public override string GetAudioFormat()
        {
            return "wav";
        }

        protected override void LogMessage(string message)
        {
            Console.WriteLine($"[GPTSoVITSTTSCore] {DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}");
        }
    }
}