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
using VPet.Plugin.ModMaker.Models;
using VPet.Plugin.ModMaker.ViewModels.ModEdit.ClickTextEdit;

namespace VPet.Plugin.ModMaker.Views.ModEdit.ClickTextEdit;

/// <summary>
/// ClickTextPage.xaml 的交互逻辑
/// </summary>
public partial class ClickTextPage : Page
{
    public ClickTextPageVM ViewModel => (ClickTextPageVM)DataContext;

    public ClickTextPage()
    {
        InitializeComponent();
        DataContext = new ClickTextPageVM();
    }

    private void DataGrid_ClickText_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (
            sender is not DataGrid dataGrid || dataGrid.SelectedItem is not ClickTextModel clickText
        )
            return;
        ViewModel.EditClickText(clickText);
    }
}
