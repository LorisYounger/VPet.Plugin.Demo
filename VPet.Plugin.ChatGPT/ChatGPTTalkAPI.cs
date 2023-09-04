using LinePutScript.Localization.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.ChatGPTPlugin
{
    public class ChatGPTTalkAPI : TalkBox
    {
        public ChatGPTTalkAPI(ChatGPTPlugin mainPlugin) : base(mainPlugin)
        {
            Plugin = mainPlugin;
        }
        protected ChatGPTPlugin Plugin;
        public override string APIName => "ChatGPT";
        public static string[] like_str = new string[] { "陌生", "普通", "喜欢", "爱" };
        public static int like_ts(int like)
        {
            if (like > 50)
            {
                if (like < 100)
                    return 1;
                else if (like < 200)
                    return 2;
                else
                    return 3;
            }
            return 0;
        }
        public override void Responded(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return;
            }
            if (Plugin.CGPTClient == null)
            {
                Plugin.MW.Main.SayRnd("请先前往设置中设置 ChatGPT API".Translate());
                return;
            }
            Dispatcher.Invoke(() => this.IsEnabled = false);
            try
            {
                if (Plugin.CGPTClient.Completions.TryGetValue("vpet", out var vpetapi))
                {
                    var last = vpetapi.messages.LastOrDefault();
                    if (last != null)
                    {
                        if (last.role == ChatGPT.API.Framework.Message.RoleType.user)
                        {
                            vpetapi.messages.Remove(last);
                        }
                    }
                }
                content = "[当前状态: {0}, 好感度:{1}({2})]".Translate(Plugin.MW.Core.Save.Mode.ToString().Translate(), like_str[like_ts((int)Plugin.MW.Core.Save.Likability)].Translate(), (int)Plugin.MW.Core.Save.Likability) + content;
                var resp = Plugin.CGPTClient.Ask("vpet", content);
                var reply = resp.GetMessageContent();
                if (resp.choices[0].finish_reason == "length")
                {
                    reply += " ...";
                }
                var showtxt = "当前Token使用".Translate() + ": " + resp.usage.total_tokens;
                Dispatcher.Invoke(() =>
                {
                    Plugin.MW.Main.MsgBar.MessageBoxContent.Children.Add(new TextBlock() { Text = showtxt, FontSize = 20, ToolTip = showtxt, HorizontalAlignment = System.Windows.HorizontalAlignment.Right });
                });
                Plugin.MW.Main.SayRnd(reply);
            }
            catch (Exception exp)
            {
                var e = exp.ToString();
                string str = "请检查设置和网络连接".Translate();
                if (e.Contains("401"))
                {
                    str = "请检查API token设置".Translate();
                }
                Plugin.MW.Main.SayRnd("API调用失败".Translate() + $",{str}\n{e}");//, GraphCore.Helper.SayType.Serious);
            }
            Dispatcher.Invoke(() => this.IsEnabled = true);
        }

        public override void Setting() => Plugin.Setting();
    }
}
