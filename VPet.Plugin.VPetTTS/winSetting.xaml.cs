using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LinePutScript.Converter;
using LinePutScript.Localization.WPF;

namespace VPet.Plugin.VPetTTS
{
    /// <summary>
    /// winSetting.xaml 的交互逻辑
    /// </summary>
    public partial class winSetting : Window
    {
        VPetTTS vts;
        private Setting originalSettings;
        
        // 样式颜色
        private static readonly SolidColorBrush SubTextColor = new SolidColorBrush(Color.FromRgb(0x66, 0x66, 0x66));
        private static readonly SolidColorBrush BorderColor = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0));

        public winSetting(VPetTTS vts)
        {
            InitializeComponent();

            this.vts = vts;
            
            // 备份原始设置
            originalSettings = LPSConvert.DeserializeObject<Setting>(LPSConvert.SerializeObject(vts.Set, "VPetTTS"));
            
            LoadSettings();
            SetupEventHandlers();
        }

        private void LoadSettings()
        {
            // 基本设置
            SwitchOn.IsChecked = vts.Set.Enable;
            VolumeSilder.Value = vts.Set.Volume;
            SpeedSilder.Value = vts.Set.Speed;
            EnableCache.IsChecked = vts.Set.EnableCache;

            // 提供商选择
            foreach (ComboBoxItem item in CombProvider.Items)
            {
                if (item.Tag.ToString() == vts.Set.Provider)
                {
                    CombProvider.SelectedItem = item;
                    break;
                }
            }

            // 代理设置
            EnableProxy.IsChecked = vts.Set.Proxy.IsEnabled;
            FollowSystemProxy.IsChecked = vts.Set.Proxy.FollowSystemProxy;
            ProxyAddress.Text = vts.Set.Proxy.Address;
            
            foreach (ComboBoxItem item in ProxyProtocol.Items)
            {
                if (item.Tag.ToString() == vts.Set.Proxy.Protocol)
                {
                    ProxyProtocol.SelectedItem = item;
                    break;
                }
            }

            UpdateProviderConfig();
        }

        private void SetupEventHandlers()
        {
            VolumeSilder.ValueChanged += (s, e) => VolumeText.Text = $"{e.NewValue:F0}%";
            SpeedSilder.ValueChanged += (s, e) => SpeedText.Text = $"{e.NewValue:F1}x";
            CombProvider.SelectionChanged += (s, e) => UpdateProviderConfig();
        }

        private void UpdateProviderConfig()
        {
            ProviderConfigPanel.Children.Clear();

            if (CombProvider.SelectedItem is ComboBoxItem selectedItem)
            {
                var provider = selectedItem.Tag.ToString();
                
                switch (provider)
                {
                    case "Free":
                        AddFreeConfig();
                        break;
                    case "OpenAI":
                        AddOpenAIConfig();
                        break;
                    case "GPT-SoVITS":
                        AddGPTSoVITSConfig();
                        break;
                    case "URL":
                        AddURLConfig();
                        break;
                    case "DIY":
                        AddDIYConfig();
                        break;
                }
            }
        }

        private void AddFreeConfig()
        {
            var infoText = new TextBlock 
            { 
                Text = "🆓 " + "Free TTS 使用免费在线服务，无需配置".Translate(),
                Foreground = SubTextColor,
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 12)
            };
            ProviderConfigPanel.Children.Add(infoText);

            var langLabel = new TextBlock 
            { 
                Text = "🌍 " + "语言设置".Translate(), 
                Foreground = SubTextColor,
                FontSize = 13,
                Margin = new Thickness(0, 0, 0, 6) 
            };
            ProviderConfigPanel.Children.Add(langLabel);
            
            var langCombo = new ComboBox 
            { 
                Name = "Free_TextLanguage", 
                Margin = new Thickness(0, 0, 0, 12),
                Padding = new Thickness(10, 8, 10, 8),
                FontSize = 13,
                MinHeight = 36,
                VerticalContentAlignment = VerticalAlignment.Center
            };
            
            foreach (var lang in FreeTTSSetting.SupportedLanguages)
            {
                var item = new ComboBoxItem { Content = lang.Value.Translate(), Tag = lang.Key };
                langCombo.Items.Add(item);
                if (lang.Key == vts.Set.Free.TextLanguage)
                {
                    langCombo.SelectedItem = item;
                }
            }
            
            if (langCombo.SelectedItem == null && langCombo.Items.Count > 0)
            {
                langCombo.SelectedIndex = 0;
            }
            
            ProviderConfigPanel.Children.Add(langCombo);

            var hint = new TextBlock 
            { 
                Text = "💡 auto: 自动检测 | zh: 中文 | en: 英语 | ja: 日语 | yue: 粤语 | ko: 韩语".Translate(),
                Foreground = new SolidColorBrush(Color.FromRgb(0x99, 0x99, 0x99)),
                FontSize = 11,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 0)
            };
            ProviderConfigPanel.Children.Add(hint);
        }

        private void AddOpenAIConfig()
        {
            AddConfigLabel("🔑 API Key");
            var apiKeyBox = CreateTextBox("OpenAI_ApiKey", vts.Set.OpenAI.ApiKey);
            ProviderConfigPanel.Children.Add(apiKeyBox);

            AddConfigLabel("🌐 Base URL");
            var baseUrlBox = CreateTextBox("OpenAI_BaseUrl", vts.Set.OpenAI.BaseUrl);
            ProviderConfigPanel.Children.Add(baseUrlBox);

            AddConfigLabel("🤖 Model");
            var modelBox = CreateTextBox("OpenAI_Model", vts.Set.OpenAI.Model);
            ProviderConfigPanel.Children.Add(modelBox);

            AddConfigLabel("🎙️ Voice");
            var voiceBox = CreateTextBox("OpenAI_Voice", vts.Set.OpenAI.Voice);
            ProviderConfigPanel.Children.Add(voiceBox);
        }

        private void AddGPTSoVITSConfig()
        {
            AddConfigLabel("🌐 Base URL");
            var baseUrlBox = CreateTextBox("GPTSoVITS_BaseUrl", vts.Set.GPTSoVITS.BaseUrl);
            ProviderConfigPanel.Children.Add(baseUrlBox);

            AddConfigLabel("⚙️ API 模式");
            var apiModeCombo = CreateComboBox("GPTSoVITS_ApiMode");
            apiModeCombo.Items.Add(new ComboBoxItem { Content = "WebUI", Tag = "WebUI" });
            apiModeCombo.Items.Add(new ComboBoxItem { Content = "API v2", Tag = "ApiV2" });
            foreach (ComboBoxItem item in apiModeCombo.Items)
            {
                if (item.Tag.ToString() == vts.Set.GPTSoVITS.ApiMode)
                {
                    apiModeCombo.SelectedItem = item;
                    break;
                }
            }
            ProviderConfigPanel.Children.Add(apiModeCombo);

            AddConfigLabel("🎵 参考音频路径");
            var referWavBox = CreateTextBox("GPTSoVITS_ReferWavPath", vts.Set.GPTSoVITS.ReferWavPath);
            ProviderConfigPanel.Children.Add(referWavBox);

            AddConfigLabel("📝 提示文本");
            var promptTextBox = CreateTextBox("GPTSoVITS_PromptText", vts.Set.GPTSoVITS.PromptText);
            ProviderConfigPanel.Children.Add(promptTextBox);
        }

        private void AddURLConfig()
        {
            AddConfigLabel("🌐 Base URL");
            var baseUrlBox = CreateTextBox("URL_BaseUrl", vts.Set.URL.BaseUrl);
            ProviderConfigPanel.Children.Add(baseUrlBox);

            AddConfigLabel("🎙️ Voice ID");
            var voiceBox = CreateTextBox("URL_Voice", vts.Set.URL.Voice);
            ProviderConfigPanel.Children.Add(voiceBox);

            AddConfigLabel("📡 HTTP 方法");
            var methodCombo = CreateComboBox("URL_Method");
            methodCombo.Items.Add(new ComboBoxItem { Content = "GET", Tag = "GET" });
            methodCombo.Items.Add(new ComboBoxItem { Content = "POST", Tag = "POST" });
            foreach (ComboBoxItem item in methodCombo.Items)
            {
                if (item.Tag.ToString() == vts.Set.URL.Method)
                {
                    methodCombo.SelectedItem = item;
                    break;
                }
            }
            ProviderConfigPanel.Children.Add(methodCombo);
        }

        private void AddDIYConfig()
        {
            AddConfigLabel("🌐 Base URL");
            var baseUrlBox = CreateTextBox("DIY_BaseUrl", vts.Set.DIY.BaseUrl);
            ProviderConfigPanel.Children.Add(baseUrlBox);

            AddConfigLabel("📡 HTTP 方法");
            var methodCombo = CreateComboBox("DIY_Method");
            methodCombo.Items.Add(new ComboBoxItem { Content = "GET", Tag = "GET" });
            methodCombo.Items.Add(new ComboBoxItem { Content = "POST", Tag = "POST" });
            foreach (ComboBoxItem item in methodCombo.Items)
            {
                if (item.Tag.ToString() == vts.Set.DIY.Method)
                {
                    methodCombo.SelectedItem = item;
                    break;
                }
            }
            ProviderConfigPanel.Children.Add(methodCombo);

            AddConfigLabel("📋 Content-Type");
            var contentTypeBox = CreateTextBox("DIY_ContentType", vts.Set.DIY.ContentType);
            ProviderConfigPanel.Children.Add(contentTypeBox);

            AddConfigLabel("📝 请求体 (使用 {text} 作为文本占位符)");
            var requestBodyBox = new TextBox 
            { 
                Name = "DIY_RequestBody", 
                Text = vts.Set.DIY.RequestBody,
                AcceptsReturn = true, 
                Height = 80, 
                Margin = new Thickness(0, 0, 0, 12),
                Padding = new Thickness(10, 8, 10, 8),
                FontSize = 13,
                BorderBrush = BorderColor,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            ProviderConfigPanel.Children.Add(requestBodyBox);
        }
        
        // 辅助方法：创建配置标签
        private void AddConfigLabel(string text)
        {
            var label = new TextBlock 
            { 
                Text = text, 
                Foreground = SubTextColor,
                FontSize = 13,
                Margin = new Thickness(0, 0, 0, 6) 
            };
            ProviderConfigPanel.Children.Add(label);
        }
        
        // 辅助方法：创建文本框
        private TextBox CreateTextBox(string name, string text)
        {
            return new TextBox 
            { 
                Name = name, 
                Text = text, 
                Margin = new Thickness(0, 0, 0, 12),
                Padding = new Thickness(10, 8, 10, 8),
                FontSize = 13,
                BorderBrush = BorderColor
            };
        }
        
        // 辅助方法：创建下拉框
        private ComboBox CreateComboBox(string name)
        {
            return new ComboBox 
            { 
                Name = name, 
                Margin = new Thickness(0, 0, 0, 12),
                Padding = new Thickness(10, 8, 10, 8),
                FontSize = 13,
                MinHeight = 36,
                VerticalContentAlignment = VerticalAlignment.Center
            };
        }

        private void SaveProviderConfig()
        {
            if (CombProvider.SelectedItem is ComboBoxItem selectedItem)
            {
                var provider = selectedItem.Tag.ToString();
                vts.Set.Provider = provider;

                switch (provider)
                {
                    case "Free":
                        SaveFreeConfig();
                        break;
                    case "OpenAI":
                        SaveOpenAIConfig();
                        break;
                    case "GPT-SoVITS":
                        SaveGPTSoVITSConfig();
                        break;
                    case "URL":
                        SaveURLConfig();
                        break;
                    case "DIY":
                        SaveDIYConfig();
                        break;
                }
            }
        }

        private void SaveFreeConfig()
        {
            var langCombo = FindComboBox("Free_TextLanguage");
            if (langCombo?.SelectedItem is ComboBoxItem item)
            {
                vts.Set.Free.TextLanguage = item.Tag.ToString();
            }
        }

        private void SaveOpenAIConfig()
        {
            vts.Set.OpenAI.ApiKey = FindTextBox("OpenAI_ApiKey")?.Text ?? "";
            vts.Set.OpenAI.BaseUrl = FindTextBox("OpenAI_BaseUrl")?.Text ?? "";
            vts.Set.OpenAI.Model = FindTextBox("OpenAI_Model")?.Text ?? "";
            vts.Set.OpenAI.Voice = FindTextBox("OpenAI_Voice")?.Text ?? "";
        }

        private void SaveGPTSoVITSConfig()
        {
            vts.Set.GPTSoVITS.BaseUrl = FindTextBox("GPTSoVITS_BaseUrl")?.Text ?? "";
            vts.Set.GPTSoVITS.ReferWavPath = FindTextBox("GPTSoVITS_ReferWavPath")?.Text ?? "";
            vts.Set.GPTSoVITS.PromptText = FindTextBox("GPTSoVITS_PromptText")?.Text ?? "";
            
            var apiModeCombo = FindComboBox("GPTSoVITS_ApiMode");
            if (apiModeCombo?.SelectedItem is ComboBoxItem item)
            {
                vts.Set.GPTSoVITS.ApiMode = item.Tag.ToString();
            }
        }

        private void SaveURLConfig()
        {
            vts.Set.URL.BaseUrl = FindTextBox("URL_BaseUrl")?.Text ?? "";
            vts.Set.URL.Voice = FindTextBox("URL_Voice")?.Text ?? "";
            
            var methodCombo = FindComboBox("URL_Method");
            if (methodCombo?.SelectedItem is ComboBoxItem item)
            {
                vts.Set.URL.Method = item.Tag.ToString();
            }
        }

        private void SaveDIYConfig()
        {
            vts.Set.DIY.BaseUrl = FindTextBox("DIY_BaseUrl")?.Text ?? "";
            vts.Set.DIY.ContentType = FindTextBox("DIY_ContentType")?.Text ?? "";
            vts.Set.DIY.RequestBody = FindTextBox("DIY_RequestBody")?.Text ?? "";
            
            var methodCombo = FindComboBox("DIY_Method");
            if (methodCombo?.SelectedItem is ComboBoxItem item)
            {
                vts.Set.DIY.Method = item.Tag.ToString();
            }
        }

        private TextBox FindTextBox(string name)
        {
            foreach (var child in ProviderConfigPanel.Children)
            {
                if (child is TextBox textBox && textBox.Name == name)
                    return textBox;
            }
            return null;
        }

        private ComboBox FindComboBox(string name)
        {
            foreach (var child in ProviderConfigPanel.Children)
            {
                if (child is ComboBox comboBox && comboBox.Name == name)
                    return comboBox;
            }
            return null;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 保存基本设置
                if (vts.Set.Enable != SwitchOn.IsChecked.Value)
                {
                    if (SwitchOn.IsChecked.Value)
                        vts.MW.Main.SayProcess.Add(vts.Main_OnSay);
                    else
                        vts.MW.Main.SayProcess.Remove(vts.Main_OnSay);
                    vts.Set.Enable = SwitchOn.IsChecked.Value;
                }

                vts.Set.Volume = VolumeSilder.Value;
                vts.Set.Speed = SpeedSilder.Value;
                vts.Set.EnableCache = EnableCache.IsChecked.Value;

                // 保存代理设置
                vts.Set.Proxy.IsEnabled = EnableProxy.IsChecked.Value;
                vts.Set.Proxy.FollowSystemProxy = FollowSystemProxy.IsChecked.Value;
                vts.Set.Proxy.Address = ProxyAddress.Text;
                if (ProxyProtocol.SelectedItem is ComboBoxItem protocolItem)
                {
                    vts.Set.Proxy.Protocol = protocolItem.Tag.ToString();
                }

                // 保存提供商配置
                SaveProviderConfig();

                // 验证并保存设置
                vts.Set.Validate();
                vts.MW.Set["VPetTTS"] = LPSConvert.SerializeObject(vts.Set, "VPetTTS");

                // 刷新 TTS 管理器设置
                vts.ttsManager.RefreshSettings();

                MessageBox.Show("设置已保存", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存设置失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            // 恢复原始设置
            vts.Set = originalSettings;
            Close();
        }

        private async void Test_Click(object sender, RoutedEventArgs e)
        {
            Test.IsEnabled = false;
            try
            {
                // 临时应用当前设置进行测试
                SaveProviderConfig();
                vts.Set.Volume = VolumeSilder.Value;
                vts.Set.Speed = SpeedSilder.Value;

                var success = await vts.TestTTSAsync();
                if (!success)
                {
                    MessageBox.Show("TTS 测试失败，请检查配置", "测试失败", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"测试失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Test.IsEnabled = true;
            }
        }

        private void ClearCache_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                vts.ClearCache();
                MessageBox.Show("缓存已清理", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"清理缓存失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            vts.winSetting = null;
        }
    }
}