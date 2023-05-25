using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VPet_Simulator.Windows.Interface;
using EdgeTTS;
using LinePutScript.Converter;
using LinePutScript;
using System.IO;
using VPet_Simulator.Core;

namespace VPet.Plugin.VPetTTS
{
    public class VPETTTS : MainPlugin
    {
        IMainWindow mw;
        EdgeTTSClient etts;
        Setting Set;
        public VPETTTS(IMainWindow mainwin) : base(mainwin)
        {
            mw = mainwin;
        }
        public override void LoadPlugin()
        {
            etts = new EdgeTTSClient();
            var line = MW.Set.FindLine("DemoClock");
            if (line == null)
            {
                Set = new Setting();
            }
            else
            {
                Set = LPSConvert.DeserializeObject<Setting>(line);
            }
            if (!Directory.Exists(GraphCore.CachePath + @"\voice"))
                Directory.CreateDirectory(GraphCore.CachePath + @"\voice");
            mw.Main.OnSay += Main_OnSay;
        }

        private void Main_OnSay(string saythings)
        {//说话语音
            var path = GraphCore.CachePath + $"\\voice\\{Sub.GetHashCode(saythings):X}.mp3";
            if (File.Exists(path))
            {
                mw.Main.PlayVoice(new Uri(path));
            }else
            {
                var res = etts.SynthesisAsync(saythings, Set.Speaker, Set.PitchStr, Set.RateStr).Result;
                if (res.Code == ResultCode.Success)
                {
                    FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
                    BinaryWriter w = new BinaryWriter(fs);
                    w.Write(res.Data.ToArray());
                    fs.Close();
                    fs.Dispose();
                    w.Dispose();
                    mw.Main.PlayVoice(new Uri(path));
                }
            }            
        }

        //public override void Save()
        //{
        //    MW.Set.Remove("DemoClock");
        //    MW.Set.Add(LPSConvert.SerializeObject(Set, "DemoClock"));
        //}
        public override string PluginName => "EdgeTTS";
    }
}
