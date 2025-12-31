using LinePutScript.Converter;
using System;
using System.Collections.Generic;

namespace VPet.Plugin.VPetTTS
{
    public class Setting
    {
        /// <summary>
        /// 启用TTS
        /// </summary>
        [Line]
        public bool Enable { get; set; } = true;

        /// <summary>
        /// 当前TTS提供商
        /// </summary>
        [Line]
        public string Provider { get; set; } = "Free";

        /// <summary>
        /// 音量 (0-200)
        /// </summary>
        [Line]
        public double Volume { get; set; } = 100.0;

        /// <summary>
        /// 语速 (0.1-3.0)
        /// </summary>
        [Line]
        public double Speed { get; set; } = 1.0;

        /// <summary>
        /// 启用缓存
        /// </summary>
        [Line]
        public bool EnableCache { get; set; } = true;

        /// <summary>
        /// 代理设置
        /// </summary>
        [Line]
        public ProxySetting Proxy { get; set; } = new ProxySetting();

        /// <summary>
        /// Free TTS设置
        /// </summary>
        [Line]
        public FreeTTSSetting Free { get; set; } = new FreeTTSSetting();

        /// <summary>
        /// OpenAI TTS设置
        /// </summary>
        [Line]
        public OpenAITTSSetting OpenAI { get; set; } = new OpenAITTSSetting();

        /// <summary>
        /// GPT-SoVITS设置
        /// </summary>
        [Line]
        public GPTSoVITSTTSSetting GPTSoVITS { get; set; } = new GPTSoVITSTTSSetting();

        /// <summary>
        /// URL TTS设置
        /// </summary>
        [Line]
        public URLTTSSetting URL { get; set; } = new URLTTSSetting();

        /// <summary>
        /// DIY TTS设置
        /// </summary>
        [Line]
        public DIYTTSSetting DIY { get; set; } = new DIYTTSSetting();

        /// <summary>
        /// 验证设置
        /// </summary>
        public void Validate()
        {
            if (Volume < 0) Volume = 0;
            if (Volume > 200) Volume = 200;
            if (Speed < 0.1) Speed = 0.1;
            if (Speed > 3.0) Speed = 3.0;
            
            if (string.IsNullOrWhiteSpace(Provider))
                Provider = "Free";
        }
    }

    public class ProxySetting
    {
        [Line]
        public bool IsEnabled { get; set; } = false;
        [Line]
        public bool FollowSystemProxy { get; set; } = false;
        [Line]
        public string Protocol { get; set; } = "http";
        [Line]
        public string Address { get; set; } = "127.0.0.1:8080";
        [Line]
        public bool ForAllAPI { get; set; } = false;
        [Line]
        public bool ForTTS { get; set; } = true;
    }

    public class FreeTTSSetting
    {
        /// <summary>
        /// 文本语言设置
        /// auto=自动检测, zh=中文, en=英语, ja=日语, yue=粤语, ko=韩语
        /// </summary>
        [Line]
        public string TextLanguage { get; set; } = "auto";

        /// <summary>
        /// 获取支持的语言列表
        /// </summary>
        public static Dictionary<string, string> SupportedLanguages => new Dictionary<string, string>
        {
            { "auto", "自动检测" },
            { "zh", "中文" },
            { "en", "英语" },
            { "ja", "日语" },
            { "yue", "粤语" },
            { "ko", "韩语" }
        };
    }

    public class OpenAITTSSetting
    {
        [Line]
        public string ApiKey { get; set; } = "";
        [Line]
        public string BaseUrl { get; set; } = "https://api.openai.com/v1";
        [Line]
        public string Model { get; set; } = "tts-1";
        [Line]
        public string Voice { get; set; } = "alloy";
        [Line]
        public string Format { get; set; } = "mp3";
    }

    public class GPTSoVITSTTSSetting
    {
        [Line]
        public string BaseUrl { get; set; } = "http://127.0.0.1:9880";
        [Line]
        public string ApiMode { get; set; } = "WebUI"; // WebUI or ApiV2
        [Line]
        public string ModelName { get; set; } = "";
        [Line]
        public string ReferWavPath { get; set; } = "";
        [Line]
        public string PromptText { get; set; } = "";
        [Line]
        public string TextLanguage { get; set; } = "中文";
        [Line]
        public double Temperature { get; set; } = 1.0;
        [Line]
        public double Speed { get; set; } = 1.0;
    }

    public class URLTTSSetting
    {
        [Line]
        public string BaseUrl { get; set; } = "";
        [Line]
        public string Voice { get; set; } = "36";
        [Line]
        public string Method { get; set; } = "GET";
    }

    public class DIYTTSSetting
    {
        [Line]
        public string BaseUrl { get; set; } = "";
        [Line]
        public string Method { get; set; } = "POST";
        [Line]
        public string ContentType { get; set; } = "application/json";
        [Line]
        public string RequestBody { get; set; } = "";
        [Line]
        public List<CustomHeader> CustomHeaders { get; set; } = new();
        [Line]
        public string ResponseFormat { get; set; } = "mp3";
    }

    public class CustomHeader
    {
        [Line]
        public string Key { get; set; } = "";
        [Line]
        public string Value { get; set; } = "";
        [Line]
        public bool IsEnabled { get; set; } = true;
    }
}