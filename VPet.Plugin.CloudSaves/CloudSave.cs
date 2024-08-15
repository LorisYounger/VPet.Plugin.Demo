using LinePutScript.Converter;
using System.Collections.Generic;
using VPet_Simulator.Windows.Interface;
using CloudSaves.Client;
using System.Timers;
using Timer = System.Timers.Timer;
using LinePutScript.Localization.WPF;
using System.Windows.Controls;
using System.Windows;
using Panuon.WPF.UI;

namespace VPet.Plugin.CloudSaves
{
    public class CloudSave : MainPlugin
    {
        public Setting Set;
        public SavesClient SavesClient;
        public Timer BackupTimer;
        public CloudSave(IMainWindow mainwin) : base(mainwin)
        {

        }
        public override void LoadPlugin()
        {
            Set = LPSConvert.DeserializeObject<Setting>(MW.Set["CloudSave"]);
            SavesClient = new SavesClient(Set.ServerURL, MW.SteamID, Set.Passkey);
            BackupTimer = new Timer(Set.BackupTime * 60 * 1000);
            BackupTimer.Elapsed += BackupTimer_Elapsed;
            BackupTimer.AutoReset = true;
            BackupTimer.Start();
            if (string.IsNullOrEmpty(Set.ServerURL))
            {
                Task.Run(() =>
                {
                    Thread.Sleep(1000);
                    MW.Dispatcher.Invoke(() =>
                    {
                        MessageBoxX.Show("第一次使用需要设置服务器参数".Translate(), "CloudSave".Translate());
                        Setting();
                    });
                });
            }
            var menuItem = new MenuItem()
            {
                Header = "CloudSave".Translate(),
                HorizontalContentAlignment = HorizontalAlignment.Center
            };
            menuItem.Click += (s, e) => { Setting(); };
            MW.Main.ToolBar.MenuMODConfig.Items.Add(menuItem);
        }

        private void BackupTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (string.IsNullOrEmpty(Set.ServerURL))
                return;
            try
            {
                SavesClient.AddGameSave("vpet", $"data:|lv#{MW.GameSavesData.GameSave.Level}:|money#{(int)MW.GameSavesData.GameSave.Money}:|hash#{MW.GameSavesData.HashCheck}:|"
                    , "自动存档".Translate() + "-" + MW.GameSavesData.GameSave.Name, MW.GameSavesData.ToLPS().ToString());
            }
            catch
            {
                MW.Main.SayRnd("自动存档失败啦,主人记得检查下自动存档设置".Translate());
            }
        }
        winSetting winSetting;
        public override void Setting()
        {
            if (winSetting != null)
            {
                winSetting.Activate();
                return;
            }
            else
            {
                winSetting = new winSetting(this);
                winSetting.Closed += (s, e) => { winSetting = null; };
                winSetting.Show();
            }
        }
        winSave winSave;
        public void ShowSave()
        {
            if (winSave != null)
            {
                winSave.Close();
                winSave = null;
            }
            winSave = new winSave(this);
            winSave.Closed += (s, e) => { winSave = null; };
            winSave.Show();

        }
        public override string PluginName => "CloudSave";
    }
}
