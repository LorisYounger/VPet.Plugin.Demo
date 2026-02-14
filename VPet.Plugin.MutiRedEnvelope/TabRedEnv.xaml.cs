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
using System.Windows.Navigation;
using System.Windows.Shapes;
using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.MutiRedEnvelope
{
    /// <summary>
    /// TabRedEnv.xaml 的交互逻辑
    /// </summary>
    public partial class TabRedEnv : TabItem
    {
        IMPWindows IMP;


        public TabRedEnv(IMPWindows imp)
        {
            InitializeComponent();
            IMP = imp;
            IMP.TabControl.Items.Add(this);
            imp.ClosingMutiPlayer += IMP_ClosingMutiPlayer;
            imp.ReceivedMessage += IMP_ReceivedMessage;
        }
        /// <summary>
        /// 红包使用的通信频道 -192000-192999
        /// </summary>
        /// 其他联机MOD请勿使用此频道，以免冲突, 可以随便取个其他频道, 只要不和其他MOD冲突即可
        public enum MPType
        {
            /// <summary>
            /// 想要红包列表 (一般是新玩家给主机发送,获取红包列表)
            /// </summary>
            REDENV_Want_List = -192001,
            /// <summary>
            /// 红包列表 (主机发送,给玩家发送红包列表(更新数据:eg:有人领取了红包等))
            /// </summary>
            REDENV_RedList = -192002,
            /// <summary>
            /// 新红包 (主机发送,告诉所有人有新红包了)
            /// </summary>
            REDENV_NewRed = -192003,
            /// <summary>
            /// 创建红包 (玩家发送,告诉主机想要创建一个红包,然后由主机群发)
            /// </summary>
            REDENV_CreateRed = -192004,
            /// <summary>
            /// 领取红包 (玩家发送,告诉主机想要领取红包,然后由主机处理并更新红包列表RedList)
            /// </summary>
            REDENV_GetRed = -192005,
        }

        /// <summary>
        /// 收到联机消息时触发
        /// </summary>
        private void IMP_ReceivedMessage(ulong from, MPMessage message)
        {
            switch (message.Type)
            {
                case (int)MPType.REDENV_Want_List:

                    break;
            }
            //其他类型的消息不处理, 一般是通信或者其他MOD的消息
        }

        private void IMP_ClosingMutiPlayer()
        {//给红包发送者退款

        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void BtnSendRedEnv_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
