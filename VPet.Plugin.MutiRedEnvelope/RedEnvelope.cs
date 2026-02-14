using LinePutScript.Localization.WPF;
using Panuon.WPF.UI;
using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.MutiRedEnvelope
{
    public class RedEnvelope : MainPlugin
    {
        public RedEnvelope(IMainWindow mainwin) : base(mainwin)
        {
            MW.MutiPlayerHandle += MW_MutiPlayerHandle;
        }

        private void MW_MutiPlayerHandle(IMPWindows obj)
        {
            MW.Dispatcher.Invoke(() => new TabRedEnv(obj));
        }

        public override string PluginName => "RedEnvelope";

        //public override void Setting()
        //{
        //    MessageBoxX.Show("本MOD无需进行设置".Translate());
        //}
    }
}
