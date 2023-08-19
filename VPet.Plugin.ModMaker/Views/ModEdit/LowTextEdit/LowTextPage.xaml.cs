using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using VPet.Plugin.ModMaker.ViewModels.ModEdit.LowTextEdit;
using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.ModMaker.Views.ModEdit.LowTextEdit;

/// <summary>
/// Page_LowText.xaml 的交互逻辑
/// </summary>
public partial class LowTextPage : Page
{
    public LowTextPageVM ViewModel => (LowTextPageVM)DataContext;

    public LowTextPage()
    {
        InitializeComponent();
        DataContext = new LowTextPageVM();
    }

    //private void Button_AddLowText_Click(object sender, RoutedEventArgs e)
    //{
    //    var window = new LowTextEditWindow();
    //    window.Closed += (s, e) =>
    //    {
    //        if (s is not LowTextEditWindow addLowTextWindow || addLowTextWindow.IsCancel)
    //            return;
    //        var lowText = CreateLowTextFromWindow(addLowTextWindow);
    //        if (LowTextDict.TryGetValue(lowText.Text, out var oldText))
    //        {
    //            LowTextDict[lowText.Text] = LowTexts[LowTexts.IndexOf(oldText)] = lowText;
    //        }
    //        else
    //        {
    //            LowTexts.Add(lowText);
    //            LowTextDict.Add(lowText.Text, lowText);
    //        }
    //    };
    //    ShowDialogX(window);
    //}

    private void DataGrid_Food_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is not DataGrid dataGrid || dataGrid.SelectedItem is not LowTextModel lowText)
            return;
        ViewModel.EditLowText(lowText);
    }

    //private void TextBox_SearchFood_TextChanged(object sender, TextChangedEventArgs e)
    //{
    //    if (sender is not TextBox textBox)
    //        return;
    //    if (textBox.Text.Length > 0)
    //    {
    //        var newList = new ObservableCollection<LowText>(
    //            LowTexts.Where(i => i.Text.Contains(textBox.Text))
    //        );
    //        if (newList.Count != LowTexts.Count)
    //            DataGrid_LowText.ItemsSource = newList;
    //    }
    //    else
    //        DataGrid_LowText.ItemsSource = LowTexts;
    //}

    //private void MenuItem_ChangeLowText_Click(object sender, RoutedEventArgs e)
    //{
    //    if (sender is not MenuItem menuItem || menuItem.Tag is not LowText lowText)
    //        return;
    //    ChangeLowText(lowText);
    //}

    //private void MenuItem_RemoveLowText_Click(object sender, RoutedEventArgs e)
    //{
    //    if (sender is not MenuItem menuItem || menuItem.Tag is not LowText lowText)
    //        return;
    //    if (DataGrid_LowText.ItemsSource is IList list && list.Count != LowTexts.Count)
    //        list.Remove(lowText);
    //    else
    //        LowTexts.Remove(lowText);
    //    LowTextDict.Remove(lowText.Text);
    //}
}
