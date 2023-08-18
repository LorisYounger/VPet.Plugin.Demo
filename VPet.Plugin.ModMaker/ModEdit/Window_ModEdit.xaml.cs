using Microsoft.Win32;
using Panuon.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
using VPet.Plugin.ModMaker.ModEdit.FoodEdit;
using VPet.Plugin.ModMaker.ModEdit.LowTextEdit;
using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.ModMaker.ModEdit;

/// <summary>
/// winModInfo.xaml 的交互逻辑
/// </summary>
public partial class Window_ModEdit : Window
{
    public string ModName { get; set; } = string.Empty;

    public Window_ModEdit()
    {
        InitializeComponent();
        Closed += WinModInfo_Closed;
        Frame_Food.Content = InitializeFoodPage();
        Frame_LowText.Content = InitializeLowTextPage();
    }

    private Page_Food InitializeFoodPage()
    {
        var page = new Page_Food();
        page.Foods.CollectionChanged += (s, e) =>
        {
            TabItem_Food.Header = $"{TabItem_Food.Tag} ({page.Foods.Count})";
        };
        page.ShowDialogX += (w) =>
        {
            w.ShowDialogX(this);
        };
        return page;
    }

    private Page_LowText InitializeLowTextPage()
    {
        var page = new Page_LowText();
        page.LowTexts.CollectionChanged += (s, e) =>
        {
            TabItem_LowText.Header = $"{TabItem_LowText.Tag} ({page.LowTexts.Count})";
        };
        page.ShowDialogX += (w) =>
        {
            w.ShowDialogX(this);
        };
        return page;
    }

    private void WinModInfo_Closed(object sender, EventArgs e) { }

    private void Button_AddModImage_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog =
            new() { Title = "选择图片", Filter = $"图片|*.jpg;*.jpeg;*.png;*.bmp" };
        if (openFileDialog.ShowDialog() is true)
        {
            Image_ModImage.Source = new BitmapImage(new Uri(openFileDialog.FileName));
            Button_AddModImage.Visibility = Visibility.Hidden;
        }
    }

    private void MenuItem_ChangeModImage_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog =
            new() { Title = "选择图片", Filter = $"图片|*.jpg;*.jpeg;*.png;*.bmp" };
        if (openFileDialog.ShowDialog() is true)
        {
            Image_ModImage.Source = new BitmapImage(new Uri(openFileDialog.FileName));
        }
    }

    private void Button_AddItem_Click(object sender, RoutedEventArgs e) { }

    private void Button_AddAnime_Click(object sender, RoutedEventArgs e) { }

    private void Button_AddAudio_Click(object sender, RoutedEventArgs e) { }

    private void Button_AddClickText_Click(object sender, RoutedEventArgs e) { }
}
