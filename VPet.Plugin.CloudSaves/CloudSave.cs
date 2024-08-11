using LinePutScript.Converter;
using System.Collections.Generic;
using VPet_Simulator.Windows.Interface;
using CloudSaves.Client;
using System.Timers;
using Timer = System.Timers.Timer;

namespace VPet.Plugin.CloudSaves
{
    public class CloudSave : MainPlugin
    {
        Setting Set;
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
        }

        public override string PluginName => "CloudSave";
    }
}
