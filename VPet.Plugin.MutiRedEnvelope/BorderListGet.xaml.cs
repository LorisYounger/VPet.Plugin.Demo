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
using static VPet.Plugin.MutiRedEnvelope.RedMessageData;

namespace VPet.Plugin.MutiRedEnvelope
{
    /// <summary>
    /// BorderListGet.xaml 的交互逻辑
    /// </summary>
    public partial class BorderListGet : Border
    {
        public BorderListGet(IMPFriend friend, GetData data)
        {
            InitializeComponent();
            uname.Content = friend.Name;
            uimg.Source = friend.Avatar;
            utime.Content = data.GetTime.ToShortTimeString();
            umoney.Content = data.Money.ToString("f2");
            if(friend.IsMe)
            {
                Background = new SolidColorBrush(Color.FromArgb(255, 255, 132, 76));
            }
        }
        public void SetIsBest()
        {
            ubest.Visibility = Visibility.Visible;
        }
    }
}
