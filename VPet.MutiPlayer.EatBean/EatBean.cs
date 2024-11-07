
using VPet_Simulator.Windows.Interface;

namespace VPet.MutiPlayer.EatBean
{
    /// <summary>
    /// 吃豆2 堂堂回归!
    /// </summary>
    public class EatBean : MainPlugin
    {

        public int TransIndex = 1000;
        /// <summary>
        /// 传输数据
        /// </summary>
        public List<TransData> TransData = new List<TransData>();

        public EatBean(IMainWindow mainwin) : base(mainwin)
        {
            MW.MutiPlayerHandle += MW_MutiPlayerHandle;
        }
        IMPWindows IMPW;
        private void MW_MutiPlayerHandle(IMPWindows windows)
        {
            //多人联机预备处理
            IMPW = windows;

        }

        public override string PluginName => "EatBean";
    }

}
