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
using System.Windows.Shapes;

namespace VPet.Plugin.DemoClock
{
    /// <summary>
    /// CountDownInput.xaml 的交互逻辑
    /// </summary>
    public partial class CountDownInput : Window
    {
        public TimeSpan Return = TimeSpan.Zero;

        public CountDownInput(TimeSpan ret)
        {
            InitializeComponent();
            if (ret < TimeSpan.Zero)
            {
                ret = -ret;
            }
            hh.Value = Math.Min(40, (int)ret.TotalHours);
            mm.Value = ret.Minutes;
            ss.Value = ret.Seconds;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            Return = TimeSpan.FromHours(hh.Value.Value) + TimeSpan.FromSeconds(ss.Value.Value) + TimeSpan.FromMinutes(mm.Value.Value);
            DialogResult = true;
        }
    }
}
