using HKW.HKWViewModels.SimpleObservable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using VPet.Plugin.ModMaker.ViewModels.ModEdit;

namespace VPet.Plugin.ModMaker.Views.ModEdit;

/// <summary>
/// Window_AddLang.xaml 的交互逻辑
/// </summary>
public partial class Window_AddLang : Window
{
    public bool IsCancel { get; internal set; } = true;

    public ObservableCollection<string> Langs { get; internal set; }

    public ObservableValue<string> Lang { get; } = new();

    public Window_AddLang()
    {
        InitializeComponent();
        this.DataContext = this;
        TextBox_Lang.Focus();
    }

    private void Button_Cancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Button_Yes_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(Lang.Value))
        {
            MessageBox.Show("Lang不可为空", "", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (Langs.Contains(Lang.Value))
        {
            MessageBox.Show("此语言已存在", "", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        IsCancel = false;
        Close();
    }

    private void Hyperlink_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(
            new ProcessStartInfo(
                "https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-lcid/a9eac961-e77d-41a6-90a5-ce1a8b0cdb9c"
            )
        );
    }
}
