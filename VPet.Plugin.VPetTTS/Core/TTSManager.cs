using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VPet.Plugin.VPetTTS.Core.Providers;

namespace VPet.Plugin.VPetTTS.Core
{
    /// <summary>
    /// TTS管理器
    /// </summary>
    public class TTSManager
    {
        private readonly Setting _settings;
        private readonly Dictionary<string, TTSCoreBase> _providers;

        public TTSManager(Setting settings)
        {
            _settings = settings;
            _providers = new Dictionary<string, TTSCoreBase>();
            InitializeProviders();
        }

        /// <summary>
        /// 初始化TTS提供商
        /// </summary>
        private void InitializeProviders()
        {
            try
            {
                _providers["Free"] = new FreeTTSCore(_settings);
                _providers["OpenAI"] = new OpenAITTSCore(_settings);
                _providers["GPT-SoVITS"] = new GPTSoVITSTTSCore(_settings);
                _providers["URL"] = new URLTTSCore(_settings);
                _providers["DIY"] = new DIYTTSCore(_settings);
            }
            catch (Exception ex)
            {
                LogMessage($"初始化TTS提供商失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 生成音频
        /// </summary>
        public async Task<byte[]?> GenerateAudioAsync(string text)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(text))
                    return null;

                var provider = GetCurrentProvider();
                if (provider == null)
                {
                    LogMessage($"未找到TTS提供商: {_settings.Provider}");
                    return null;
                }

                LogMessage($"使用 {provider.Name} 生成音频: {text}");
                return await provider.GenerateAudioAsync(text);
            }
            catch (Exception ex)
            {
                LogMessage($"生成音频失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 获取当前提供商
        /// </summary>
        private TTSCoreBase? GetCurrentProvider()
        {
            if (_providers.TryGetValue(_settings.Provider, out var provider))
            {
                return provider;
            }
            return null;
        }

        /// <summary>
        /// 获取可用提供商列表
        /// </summary>
        public List<string> GetAvailableProviders()
        {
            return new List<string>(_providers.Keys);
        }

        /// <summary>
        /// 切换提供商
        /// </summary>
        public void SwitchProvider(string providerName)
        {
            if (_providers.ContainsKey(providerName))
            {
                _settings.Provider = providerName;
                LogMessage($"切换到TTS提供商: {providerName}");
            }
            else
            {
                LogMessage($"未知的TTS提供商: {providerName}");
            }
        }

        /// <summary>
        /// 更新 Free TTS 语言设置
        /// </summary>
        public void UpdateFreeLanguage(string language)
        {
            if (_providers.TryGetValue("Free", out var provider) && provider is FreeTTSCore freeCore)
            {
                freeCore.UpdateLanguage(language);
                LogMessage($"Free TTS 语言已更新为: {language}");
            }
        }

        /// <summary>
        /// 刷新设置（在设置更改后调用）
        /// </summary>
        public void RefreshSettings()
        {
            // 更新 Free TTS 语言设置
            UpdateFreeLanguage(_settings.Free?.TextLanguage ?? "auto");
            LogMessage("TTS 设置已刷新");
        }

        /// <summary>
        /// 记录日志
        /// </summary>
        private void LogMessage(string message)
        {
            Console.WriteLine($"[TTSManager] {DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}");
        }
    }
}