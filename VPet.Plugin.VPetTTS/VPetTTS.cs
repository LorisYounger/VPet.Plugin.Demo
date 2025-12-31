using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using VPet_Simulator.Windows.Interface;
using VPet_Simulator.Core;
using LinePutScript.Converter;
using LinePutScript;
using LinePutScript.Localization.WPF;
using VPet.Plugin.VPetTTS.Core;

namespace VPet.Plugin.VPetTTS
{
    public class VPetTTS : MainPlugin
    {
        public override string PluginName => "VPetTTS";
        
        public Setting Set;
        public TTSManager ttsManager;
        public winSetting winSetting;

        public VPetTTS(IMainWindow mainwin) : base(mainwin)
        {
        }

        public override void LoadPlugin()
        {
            // 加载设置
            Set = LPSConvert.DeserializeObject<Setting>(MW.Set["VPetTTS"]);
            Set?.Validate();

            // 创建缓存目录
            if (!Directory.Exists(GraphCore.CachePath + @"\tts"))
                Directory.CreateDirectory(GraphCore.CachePath + @"\tts");

            // 初始化Free TTS配置（异步）
            _ = Task.Run(async () =>
            {
                try
                {
                    await Utils.FreeConfigManager.InitializeTTSConfigAsync();
                    LogMessage("Free TTS 配置初始化完成");
                }
                catch (Exception ex)
                {
                    LogMessage($"Free TTS 配置初始化失败: {ex.Message}");
                }
            });

            // 初始化TTS管理器
            ttsManager = new TTSManager(Set);

            // 如果启用TTS，注册SayProcess事件
            if (Set.Enable)
                MW.Main.SayProcess.Add(Main_OnSay);

            // 添加到MOD配置菜单
            MenuItem modset = MW.Main.ToolBar.MenuMODConfig;
            modset.Visibility = Visibility.Visible;
            var menuItem = new MenuItem()
            {
                Header = "VPetTTS".Translate(),
                HorizontalContentAlignment = HorizontalAlignment.Center,
            };
            menuItem.Click += (s, e) => { Setting(); };
            modset.Items.Add(menuItem);
        }

        /// <summary>
        /// 处理说话事件
        /// </summary>
        public async void Main_OnSay(VPet_Simulator.Core.SayInfo sayInfo)
        {
            try
            {
                if (!Set.Enable)
                    return;

                // 获取说话文本
                var saythings = await sayInfo.GetSayText();
                
                if (string.IsNullOrWhiteSpace(saythings))
                    return;

                LogMessage($"处理TTS请求: {saythings}");

                // 生成缓存文件路径
                var cacheKey = Sub.GetHashCode(saythings + Set.Provider).ToString("X");
                var path = GraphCore.CachePath + $"\\tts\\{cacheKey}.mp3";

                // 检查缓存
                if (Set.EnableCache && File.Exists(path))
                {
                    MW.Main.PlayVoice(new Uri(path));
                    return;
                }

                // 生成音频
                var audioData = await ttsManager.GenerateAudioAsync(saythings);
                if (audioData != null && audioData.Length > 0)
                {
                    // 保存到缓存
                    if (Set.EnableCache)
                    {
                        await File.WriteAllBytesAsync(path, audioData);
                    }
                    else
                    {
                        // 不使用缓存时，创建临时文件
                        path = Path.GetTempFileName();
                        path = Path.ChangeExtension(path, "mp3");
                        await File.WriteAllBytesAsync(path, audioData);
                    }

                    // 播放音频
                    MW.Main.PlayVoice(new Uri(path));

                    // 如果不使用缓存，延迟删除临时文件
                    if (!Set.EnableCache)
                    {
                        _ = Task.Delay(TimeSpan.FromSeconds(10)).ContinueWith(_ =>
                        {
                            try
                            {
                                if (File.Exists(path))
                                    File.Delete(path);
                            }
                            catch { }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"TTS处理失败: {ex.Message}");
            }
        }

        public override void Setting()
        {
            if (winSetting == null || !winSetting.IsLoaded)
            {
                winSetting = new winSetting(this);
                winSetting.Show();
            }
            else
            {
                winSetting.Activate();
                winSetting.Topmost = true;
                winSetting.Topmost = false;
            }
        }

        /// <summary>
        /// 记录日志
        /// </summary>
        public void LogMessage(string message)
        {
            Console.WriteLine($"[VPetTTS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}");
        }

        /// <summary>
        /// 测试TTS功能
        /// </summary>
        public async Task<bool> TestTTSAsync(string text = null)
        {
            try
            {
                text = text ?? "你好，主人。现在是".Translate() + DateTime.Now.ToString("HH:mm");
                var audioData = await ttsManager.GenerateAudioAsync(text);
                
                if (audioData != null && audioData.Length > 0)
                {
                    var tempPath = Path.GetTempFileName();
                    tempPath = Path.ChangeExtension(tempPath, "mp3");
                    await File.WriteAllBytesAsync(tempPath, audioData);
                    
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MW.Main.PlayVoice(new Uri(tempPath));
                    });

                    // 延迟删除临时文件
                    _ = Task.Delay(TimeSpan.FromSeconds(10)).ContinueWith(_ =>
                    {
                        try
                        {
                            if (File.Exists(tempPath))
                                File.Delete(tempPath);
                        }
                        catch { }
                    });

                    return true;
                }
            }
            catch (Exception ex)
            {
                LogMessage($"TTS测试失败: {ex.Message}");
            }
            return false;
        }

        /// <summary>
        /// 清理缓存
        /// </summary>
        public void ClearCache()
        {
            try
            {
                var cacheDir = GraphCore.CachePath + @"\tts";
                if (Directory.Exists(cacheDir))
                {
                    foreach (var file in Directory.GetFiles(cacheDir))
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch { }
                    }
                }
                LogMessage("TTS缓存已清理");
            }
            catch (Exception ex)
            {
                LogMessage($"清理缓存失败: {ex.Message}");
            }
        }
    }
}