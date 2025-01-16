using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using VPet_Simulator.Core;
using VPet_Simulator.Windows.Interface;
using EdgeTTS;
using LinePutScript;
using LinePutScript.Converter;
using LinePutScript.Localization.WPF;
using Serilog;

namespace VPet.Plugin.VPetTTS
{
    public class EdgeTTS : MainPlugin
    {
        internal EdgeTTSClient etts;
        internal Setting Set;
        internal winSetting winSetting;
        private readonly object winSettingLock = new object();
        private readonly object fileLock = new object();
        private readonly object musicLock = new object();
        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        public EdgeTTS(IMainWindow mainwin) : base(mainwin)
        {
            etts = new EdgeTTSClient();
        }

        public override void LoadPlugin()
        {
            LoadSettings();
            EnsureVoiceDirectory();

            if (Set?.Enable ?? false)
            {
                lock (this)
                {
                    MW.Main.OnSay -= Main_OnSay; // Ensure we don't subscribe multiple times
                    MW.Main.OnSay += Main_OnSay;
                }
            }

            SetupMenu();
        }

        private void LoadSettings()
        {
            var settings = MW.Set["EdgeTTS"];
            Set = settings != null ? LPSConvert.DeserializeObject<Setting>(settings) : new Setting();

            if (string.IsNullOrEmpty(Set.Speaker))
            {
                var translatedSpeaker = "EdgeTTSSpeaker".Translate();
                Set.Speaker = translatedSpeaker == "EdgeTTSSpeaker" ? "en-US-AnaNeural" : translatedSpeaker;
            }
        }

        private void EnsureVoiceDirectory()
        {
            var voiceDir = Path.Combine(GraphCore.CachePath, "voice");
            if (!Directory.Exists(voiceDir))
            {
                Directory.CreateDirectory(voiceDir);
            }
        }

        private void SetupMenu()
        {
            var modset = MW.Main.ToolBar.MenuMODConfig;
            if (modset != null)
            {
                modset.Visibility = Visibility.Visible;

                var menuItem = new MenuItem
                {
                    Header = "EdgeTTS",
                    HorizontalContentAlignment = HorizontalAlignment.Center
                };
                menuItem.Click += (s, e) => Setting();
                modset.Items.Add(menuItem);
            }
        }

        public async void Main_OnSay(string saythings)
        {
            if (string.IsNullOrWhiteSpace(saythings))
                return;

            var path = Path.Combine(GraphCore.CachePath, "voice", $"{Sub.GetHashCode(saythings):X}.mp3");
            if (File.Exists(path) && IsValidAudioFile(path))
            {
                lock (musicLock)
                {
                    MW.Main.PlayVoice(new Uri(path));
                }
            }
            else
            {
                await semaphoreSlim.WaitAsync();
                try
                {
                    var res = await etts.SynthesisAsync(saythings, Set.Speaker, Set.PitchStr, Set.RateStr).ConfigureAwait(false);
                    if (res.Code == ResultCode.Success)
                    {
                        await SaveAudioToFileAsync(path, res.Data).ConfigureAwait(false);

                        if (IsValidAudioFile(path)) // Verify after saving
                        {
                            lock (musicLock)
                            {
                                MW.Main.PlayVoice(new Uri(path));
                            }
                        }
                        else
                        {
                            Log.Error($"The saved audio file is invalid: {path}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Error during text-to-speech synthesis: {ex.Message}");
                }
                finally
                {
                    semaphoreSlim.Release();
                }
            }
        }

        private bool IsValidAudioFile(string path)
        {
            try
            {
                using (var mp3Stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    if (mp3Stream.Length == 0)
                        return false;

                    byte[] buffer = new byte[3];
                    mp3Stream.Read(buffer, 0, 3);
                    return buffer[0] == 0xFF && (buffer[1] & 0xE0) == 0xE0; // Simple MP3 header check
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Invalid audio file: {path}. Error: {ex.Message}");
                return false;
            }
        }

        private async Task SaveAudioToFileAsync(string path, MemoryStream audioData)
        {
            try
            {
                lock (fileLock) // 确保在写入文件时不会有其他线程干扰
                {
                    using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        var audioBytes = audioData.ToArray();
                        fs.Write(audioBytes, 0, audioBytes.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to save audio to file: {ex.Message}");
            }
        }

        public override void Setting()
        {
            lock (winSettingLock)
            {
                if (winSetting == null)
                {
                    winSetting = new winSetting(this);
                    winSetting.Closed += (s, e) =>
                    {
                        lock (winSettingLock)
                        {
                            winSetting = null;
                        }
                    };
                    winSetting.Show();
                }
                else
                {
                    winSetting.Activate(); // Bring the window to the front if it is already open
                }
            }
        }

        public override string PluginName => "EdgeTTS";
    }
}
