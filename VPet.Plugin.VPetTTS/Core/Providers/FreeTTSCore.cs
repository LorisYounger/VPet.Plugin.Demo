using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VPet.Plugin.VPetTTS.Utils;

namespace VPet.Plugin.VPetTTS.Core.Providers
{
    /// <summary>
    /// Free TTS 实现 (POST API 格式)
    /// 配置从服务器动态获取
    /// </summary>
    public class FreeTTSCore : TTSCoreBase
    {
        public override string Name => "Free";

        private string _apiKey;
        private string _apiUrl;
        private string _model;
        private string _textLanguage;

        public FreeTTSCore(Setting settings) : base(settings)
        {
            LoadConfig();
            _textLanguage = settings.Free?.TextLanguage ?? "auto";
        }

        /// <summary>
        /// 更新语言设置
        /// </summary>
        public void UpdateLanguage(string language)
        {
            _textLanguage = language ?? "auto";
        }

        private void LoadConfig()
        {
            try
            {
                var config = FreeConfigManager.GetTTSConfig();
                if (config != null)
                {
                    _apiKey = DecodeString(config["API_KEY"]?.ToString() ?? "");
                    _apiUrl = DecodeString(config["API_URL"]?.ToString() ?? "");
                    _model = config["Model"]?.ToString() ?? "";
                    LogMessage("FreeTTSCore: 配置加载成功");
                }
                else
                {
                    LogMessage("FreeTTSCore: 配置文件不存在，请等待配置下载完成后重启程序");
                    _apiKey = "";
                    _apiUrl = "";
                    _model = "";
                }
            }
            catch (Exception ex)
            {
                LogMessage($"FreeTTSCore: 加载配置失败: {ex.Message}");
                _apiKey = "";
                _apiUrl = "";
                _model = "";
            }
        }

        public override async Task<byte[]> GenerateAudioAsync(string text)
        {
            try
            {
                if (string.IsNullOrEmpty(_apiUrl) || string.IsNullOrEmpty(_apiKey))
                {
                    LogMessage("TTS (Free): 配置未加载，TTS功能不可用");
                    OnAudioGenerationError("Free TTS 配置未加载，请等待配置下载完成后重启程序");
                    return Array.Empty<byte>();
                }

                LogMessage($"TTS (Free): 发送请求，文本长度: {text.Length}");

                // 构建请求体
                var requestBody = new
                {
                    text = text,
                    text_lang = _textLanguage,
                    api_key = _apiKey,
                };

                LogMessage($"TTS (Free): 使用语言: {_textLanguage}");

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var startTime = DateTime.Now;
                using var client = CreateHttpClient();
                var response = await client.PostAsync(_apiUrl, content);
                var elapsed = (DateTime.Now - startTime).TotalSeconds;

                LogMessage($"TTS (Free): 响应接收完成，耗时 {elapsed:F2} 秒, 状态: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    LogMessage($"TTS (Free): API 错误: {response.StatusCode} - {errorContent}");

                    // 尝试解析错误消息（符合文档中的错误响应格式）
                    try
                    {
                        var errorObj = JObject.Parse(errorContent);
                        var errorMessage = errorObj["message"]?.ToString() ?? "未知错误";
                        OnAudioGenerationError($"Free TTS 服务错误: {errorMessage}");
                    }
                    catch
                    {
                        OnAudioGenerationError($"Free TTS 服务错误: {response.StatusCode}");
                    }

                    return Array.Empty<byte>();
                }

                // 成功响应：二进制音频数据（Content-Type: audio/wav）
                var audioData = await response.Content.ReadAsByteArrayAsync();
                LogMessage($"TTS (Free): 音频生成成功，大小: {audioData.Length} bytes");

                OnAudioGenerated(audioData);
                return audioData;
            }
            catch (TaskCanceledException ex)
            {
                LogMessage($"TTS (Free): 请求超时: {ex.Message}");
                OnAudioGenerationError("请求超时，请检查网络连接");
                return Array.Empty<byte>();
            }
            catch (HttpRequestException ex)
            {
                LogMessage($"TTS (Free): 网络错误: {ex.Message}");
                OnAudioGenerationError($"网络错误: {ex.Message}");
                return Array.Empty<byte>();
            }
            catch (Exception ex)
            {
                LogMessage($"TTS (Free): 生成音频异常: {ex.Message}");
                OnAudioGenerationError($"生成音频异常: {ex.Message}");
                return Array.Empty<byte>();
            }
        }

        public override string GetAudioFormat()
        {
            return "wav"; // 固定返回 WAV 格式
        }

        // 该API服务由 @ycxom 提供，我们无法支持大量请求，若您拿到并且正确响应，还请不要泄露与滥用，作为 VPetLLM 为 VPet 大量AI对话Mod的其中一个免费提供AI对话服务的Mod，还请您善待，谢谢！
        private string DecodeString(string encodedString)
        {
            try
            {
                if (string.IsNullOrEmpty(encodedString))
                {
                    return "";
                }

                // 第一步：Hex解码
                var hexBytes = new byte[encodedString.Length / 2];
                for (int i = 0; i < hexBytes.Length; i++)
                {
                    hexBytes[i] = Convert.ToByte(encodedString.Substring(i * 2, 2), 16);
                }

                // 第二步：Base64解码
                var base64String = Encoding.UTF8.GetString(hexBytes);
                var finalBytes = Convert.FromBase64String(base64String);
                var result = Encoding.UTF8.GetString(finalBytes);

                return result;
            }
            catch (Exception)
            {
                return "";
            }
        }

        protected override void LogMessage(string message)
        {
            Console.WriteLine($"[FreeTTSCore] {DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}");
        }
    }
}