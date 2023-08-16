using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace VPet.Plugin.ModMaker
{
    /// <summary>
    /// winModMaker.xaml 的交互逻辑
    /// </summary>
    public partial class winModMaker : Window
    {
        ModMaker mm;
        public winModMaker(ModMaker mm)
        {
            InitializeComponent();
            this.mm = mm;
        }
    }
}
