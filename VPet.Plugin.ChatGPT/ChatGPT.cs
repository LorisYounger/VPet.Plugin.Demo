using ChatGPT.API.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.ChatGPTPlugin
{
    public class ChatGPTPlugin : MainPlugin
    {
        public ChatGPTPlugin(IMainWindow mainwin) : base(mainwin) { }
        public ChatGPTClient CGPTClient;
        public override void LoadPlugin()
        {
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\ChatGPTSetting.json"))
                CGPTClient = ChatGPTClient.Load(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"\ChatGPTSetting.json"));
            MW.TalkAPI.Add(new ChatGPTTalkAPI(this));
        }
        public override void Save()
        {
            if (CGPTClient != null)
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + @"\ChatGPTSetting.json", CGPTClient.Save());
        }
        public override void Setting()
        {
            new winSetting(this).ShowDialog();
        }
        public override string PluginName => "ChatGPT";
    }
}
