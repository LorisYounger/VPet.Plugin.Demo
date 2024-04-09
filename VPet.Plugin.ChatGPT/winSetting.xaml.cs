using ChatGPT.API.Framework;
using LinePutScript.Localization.WPF;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.ChatGPTPlugin
{
    /// <summary>
    /// winSetting.xaml 的交互逻辑
    /// </summary>
    public partial class winSetting : Window
    {
        ChatGPTPlugin plugin;
        long totalused = 0;
        public winSetting(ChatGPTPlugin plugin)
        {
            InitializeComponent();
            Resources = Application.Current.Resources;

            this.plugin = plugin;
            if (plugin.CGPTClient != null)
            {
                tbAPIKey.Text = plugin.CGPTClient.APIKey;
                tbAPIURL.Text = plugin.CGPTClient.APIUrl;
                tbMaxToken.Text = plugin.CGPTClient.Completions["vpet"].max_tokens.ToString();
                tbSystem.Text = plugin.CGPTClient.Completions["vpet"].messages[0].content;
                tbTemp.Text = plugin.CGPTClient.Completions["vpet"].temperature.ToString();
                cbModel.Text = plugin.CGPTClient.Completions["vpet"].model;
                var msgs = plugin.CGPTClient.Completions["vpet"].messages.ToList();
                msgs.RemoveAt(0);
                tbHistory.Text = JsonConvert.SerializeObject(msgs);
                lbSpend.Content = plugin.CGPTClient.TotalTokensUsage.ToString() + " Token";
                totalused = plugin.CGPTClient.TotalTokensUsage;
            }
            else
                tbSystem.Text = "请使用口语化的、可爱的、女性化的、调皮的语言风格和我交流. 你是一只桌宠, 你的名字叫{Name}, 你喜欢玩 虚拟主播模拟器. 你需要尝试体会我与你对话中的感情、产生自己的情感并尽可能给出可爱的回答".Translate();
            niKeepHistory.Value = plugin.KeepHistory;
            swShowToken.IsChecked = plugin.ShowToken;
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (tbAPIURL.Text.Split('/').Length <= 2 && !tbAPIURL.Text.Contains("completions"))
            {
                tbAPIURL.Text += "/v1/chat/completions";
            }
            plugin.CGPTClient = new ChatGPTClient(tbAPIKey.Text, tbAPIURL.Text)
            {
                TotalTokensUsage = totalused
            };
            plugin.CGPTClient.CreateCompletions("vpet", tbSystem.Text.Replace("{Name}", plugin.MW.Core.Save.Name));
            plugin.CGPTClient.Completions["vpet"].model = cbModel.Text;
            plugin.CGPTClient.Completions["vpet"].frequency_penalty = 0.2;
            plugin.CGPTClient.Completions["vpet"].presence_penalty = 1;
            plugin.CGPTClient.Completions["vpet"].max_tokens = Math.Min(Math.Max(int.Parse(tbMaxToken.Text), 10), 4000);
            plugin.CGPTClient.Completions["vpet"].temperature = Math.Min(Math.Max(double.Parse(tbTemp.Text), 0.1), 2);
            var l = JsonConvert.DeserializeObject<List<Message>>(tbHistory.Text);
            if (l != null)
                plugin.CGPTClient.Completions["vpet"].messages.AddRange(l);
            plugin.KeepHistory = (int)niKeepHistory.Value.Value;
            plugin.ShowToken = (bool)swShowToken.IsChecked;
            plugin.Save();
            this.Close();
        }
    }
}

