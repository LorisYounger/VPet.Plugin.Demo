using ChatGPT.API.Framework;
using LinePutScript.Localization.WPF;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
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
        private readonly ObservableCollection<EditableMessage> editableHistoryMessages = new ObservableCollection<EditableMessage>();
        public List<Message.RoleType> RoleOptions { get; } = Enum.GetValues(typeof(Message.RoleType)).Cast<Message.RoleType>().ToList();

        private class EditableMessage
        {
            public Message.RoleType Role { get; set; }
            public string Content { get; set; } = string.Empty;
            public string ReasoningContent { get; set; } = string.Empty;
        }

        public winSetting(ChatGPTPlugin plugin)
        {
            InitializeComponent();
            Resources = Application.Current.Resources;
            lbHistoryEditor.ItemsSource = editableHistoryMessages;

            this.plugin = plugin;
            if (plugin.CGPTClient != null)
            {
                tbAPIKey.Text = plugin.CGPTClient.APIKey;
                tbAPIURL.Text = plugin.CGPTClient.APIUrl;
                tbWebProxy.Text = plugin.WebProxy;
                if (plugin.CGPTClient.Completions["vpet"] != null)
                {
                    tbMaxToken.Text = plugin.CGPTClient.Completions["vpet"].max_tokens.ToString();
                    tbSystem.Text = plugin.CGPTClient.Completions["vpet"].messages[0].content;
                    tbTemp.Text = plugin.CGPTClient.Completions["vpet"].temperature.ToString();
                    cbModel.Text = plugin.CGPTClient.Completions["vpet"].model;
                    var msgs = plugin.CGPTClient.Completions["vpet"].messages.ToList();
                    msgs.RemoveAt(0);
                    //tbHistory.Text = JsonConvert.SerializeObject(msgs);
                    lbSpend.Content = plugin.CGPTClient.TotalTokensUsage.ToString() + " Token";
                    totalused = plugin.CGPTClient.TotalTokensUsage;
                }
            }
            else
                tbSystem.Text = "请使用口语化的、可爱的、女性化的、调皮的语言风格和我交流. 你是一只桌宠, 你的名字叫{Name}, 你喜欢玩 虚拟主播模拟器. 你需要尝试体会我与你对话中的感情、产生自己的情感并尽可能给出可爱的回答".Translate();
            cbModel.SelectionChanged += CbModel_SelectionChanged;
            niKeepHistory.Value = plugin.KeepHistory;
            swShowToken.IsChecked = plugin.ShowToken;
            swStream.IsChecked = plugin.UseStream;
            swThink.IsChecked = plugin.CGPTClient?.Completions["vpet"]?.thinking?.type == "enabled";
            tbReasoning.Text = plugin.CGPTClient?.Completions["vpet"]?.reasoning_effort ?? "high";
            LoadHistoryMessages();
        }

        private void LoadHistoryMessages()
        {
            if (plugin?.CGPTClient == null
                || !plugin.CGPTClient.Completions.TryGetValue("vpet", out var vpetCompletion)
                || vpetCompletion?.messages == null
                || vpetCompletion.messages.Count == 0)
            {
                editableHistoryMessages.Clear();
                tbNoHistory.Visibility = Visibility.Visible;
                return;
            }

            editableHistoryMessages.Clear();
            foreach (var message in vpetCompletion.messages.Skip(1))
            {
                editableHistoryMessages.Add(new EditableMessage
                {
                    Role = message.role,
                    Content = message.content ?? string.Empty,
                    ReasoningContent = message.reasoning_content ?? string.Empty
                });
            }
            tbNoHistory.Visibility = editableHistoryMessages.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void btnAddHistory_Click(object sender, RoutedEventArgs e)
        {
            editableHistoryMessages.Add(new EditableMessage
            {
                Role = Message.RoleType.user,
                Content = string.Empty,
                ReasoningContent = string.Empty
            });
            tbNoHistory.Visibility = Visibility.Collapsed;
        }

        private void btnRemoveHistory_Click(object sender, RoutedEventArgs e)
        {
            if (lbHistoryEditor.SelectedItem is EditableMessage selected)
            {
                editableHistoryMessages.Remove(selected);
                tbNoHistory.Visibility = editableHistoryMessages.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void btnRefreshHistory_Click(object sender, RoutedEventArgs e)
        {
            LoadHistoryMessages();
        }

        private void CbModel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var modelId = cbModel.Text?.Trim();
            if (string.IsNullOrWhiteSpace(modelId))
                return;

            if (modelId.StartsWith("gpt-"))
            {
                tbAPIURL.Text = "https://api.openai.com/v1/chat/completions";
            }
            else if (modelId.StartsWith("glm-"))
            {
                tbAPIURL.Text = "https://open.bigmodel.cn/api/paas/v4/chat/completions";
            }
            else if (modelId.StartsWith("deepseek-"))
            {
                tbAPIURL.Text = "https://api.deepseek.com/v1/chat/completions";
            }
            else if (modelId.StartsWith("gemini-"))
            {
                tbAPIURL.Text = "https://generativelanguage.googleapis.com/v1beta/openai/";
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            //if (tbAPIURL.Text.Split('/').Length <= 2 && !tbAPIURL.Text.Contains("completions"))
            //{
            //    tbAPIURL.Text += "/v1/chat/completions";
            //}
            plugin.CGPTClient = new ChatGPTClient(tbAPIKey.Text, tbAPIURL.Text)
            {
                TotalTokensUsage = totalused
            };
            plugin.CGPTClient.CreateCompletions("vpet", tbSystem.Text.Replace("{Name}", plugin.MW.Core.Save.Name));
            if (!string.IsNullOrWhiteSpace(tbWebProxy.Text))
            {
                plugin.WebProxy = tbWebProxy.Text;
                plugin.CGPTClient.WebProxy = tbWebProxy.Text;
                plugin.CGPTClient.Proxy = new HttpClientHandler()
                {
                    Proxy = new WebProxy(plugin.WebProxy),
                    UseProxy = true
                };
            }
            else
            {
                plugin.WebProxy = "";
                plugin.CGPTClient.WebProxy = "";
                plugin.CGPTClient.Proxy = null;
            }
            plugin.CGPTClient.Completions["vpet"].model = cbModel.Text;
#pragma warning disable CS0612 // 类型或成员已过时
            plugin.CGPTClient.Completions["vpet"].frequency_penalty = null;
            plugin.CGPTClient.Completions["vpet"].presence_penalty = null;
#pragma warning restore CS0612 // 类型或成员已过时
            plugin.CGPTClient.Completions["vpet"].max_tokens = Math.Min(Math.Max(int.Parse(tbMaxToken.Text), 10), 4000);
            plugin.CGPTClient.Completions["vpet"].temperature = Math.Min(Math.Max(double.Parse(tbTemp.Text), 0.1), 2);
            if (swThink.IsChecked == true)
            {
                plugin.CGPTClient.Completions["vpet"].thinking = new Completions.Thinking() { type = "enabled" };
            }
            else
            {
                plugin.CGPTClient.Completions["vpet"].thinking = new Completions.Thinking() { type = "disabled" };
            }
            plugin.CGPTClient.Completions["vpet"].reasoning_effort = tbReasoning.Text;
            plugin.KeepHistory = (int)niKeepHistory.Value.Value;
            plugin.ShowToken = (bool)swShowToken.IsChecked;
            plugin.UseStream = (bool)swStream.IsChecked;
            plugin.Save();
            this.Close();
        }

        private void btnSaveHistory_Click(object sender, RoutedEventArgs e)
        {
            plugin.CGPTClient.Completions["vpet"].messages.AddRange(editableHistoryMessages
                .Where(message => !string.IsNullOrWhiteSpace(message.Content)
                    || !string.IsNullOrWhiteSpace(message.ReasoningContent))
                .Select(message => new Message
                {
                    role = message.Role,
                    content = message.Content,
                    reasoning_content = message.ReasoningContent
                }));
            plugin.Save();
        }
    }
}

