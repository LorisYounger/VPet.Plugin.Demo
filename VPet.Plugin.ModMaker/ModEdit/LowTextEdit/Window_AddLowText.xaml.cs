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

namespace VPet.Plugin.ModMaker.ModEdit.LowTextEdit;

/// <summary>
/// Window_AddLowText.xaml 的交互逻辑
/// </summary>
public partial class Window_AddLowText : Window
{
    public bool IsCancel { get; internal set; } = true;

    public Window_AddLowText()
    {
        InitializeComponent();
    }

    private void Button_Cancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Button_Yes_Click(object sender, RoutedEventArgs e)
    {
        IsCancel = false;
        Close();
    }
}
