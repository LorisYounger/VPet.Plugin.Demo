using ChatGPT.API.Framework;
using LinePutScript;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.ChatGPTPlugin
{
    public class ChatGPTPlugin : MainPlugin
    {
        public ChatGPTPlugin(IMainWindow mainwin) : base(mainwin) { }
        public ChatGPTClient CGPTClient;
        public override void LoadPlugin()
        {
            if (File.Exists(ExtensionValue.BaseDirectory + @"\ChatGPTSetting.json"))
                CGPTClient = ChatGPTClient.Load(File.ReadAllText(ExtensionValue.BaseDirectory + @"\ChatGPTSetting.json"));
            MW.TalkAPI.Add(new ChatGPTTalkAPI(this));
            var menuItem = new MenuItem()
            {
                Header = "ChatGPTAPI",
                HorizontalContentAlignment = HorizontalAlignment.Center
            };
            menuItem.Click += (s, e) => { Setting(); };
            MW.Main.ToolBar.MenuMODConfig.Items.Add(menuItem);
        }
        public override void Save()
        {
            if (CGPTClient != null)
                File.WriteAllText(ExtensionValue.BaseDirectory + @"\ChatGPTSetting.json", CGPTClient.Save());
        }
        public override void Setting()
        {
            new winSetting(this).ShowDialog();
        }
        public override string PluginName => "ChatGPT";
        /// <summary>
        /// 是否在聊天位置显示Token数量
        /// </summary>
        public bool ShowToken
        {
            get => !MW.Set["CGPT"][(gbol)"noshowtoken"];
            set => MW.Set["CGPT"][(gbol)"noshowtoken"] = !value;
        }
        /// <summary>
        /// 保留的历史数量
        /// </summary>
        public int KeepHistory
        {
            get => MW.Set["CGPT"].GetInt("keephistory", 20);
            set => MW.Set["CGPT"][(gint)"keephistory"] = value;
        }
    }
}
