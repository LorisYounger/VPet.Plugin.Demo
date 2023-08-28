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
using VPet.Plugin.ModMaker.ViewModels.ModEdit.ClickTextEdit;

namespace VPet.Plugin.ModMaker.Views.ModEdit.ClickTextEdit;

/// <summary>
/// ClickTextWindow.xaml 的交互逻辑
/// </summary>
public partial class ClickTextEditWindow : Window
{
    public bool IsCancel { get; private set; } = true;
    public ClickTextEditWindowVM ViewModel => (ClickTextEditWindowVM)DataContext;

    public ClickTextEditWindow()
    {
        InitializeComponent();
        DataContext = new ClickTextEditWindowVM();
    }

    private void Button_Cancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Button_Yes_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(ViewModel.ClickText.Value.Id.Value))
        {
            MessageBox.Show("Id不可为空", "", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (ViewModel.ClickTexts.Any(i => i.Id.Value == ViewModel.ClickText.Value.Id.Value))
        {
            MessageBox.Show("此Id已存在", "", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        IsCancel = false;
        Close();
    }
}
