using Microsoft.Win32;
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

namespace VPet.Plugin.ModMaker.WinModEdit;

/// <summary>
/// AddFoodWindow.xaml 的交互逻辑
/// </summary>
public partial class AddFoodWindow : Window
{
    public bool IsCancel { get; internal set; } = true;
    public BitmapImage FoodImage { get; internal set; }

    public AddFoodWindow()
    {
        InitializeComponent();
    }

    private void Button_AddFoodImage_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog =
            new() { Title = "选择图片", Filter = $"图片|*.jpg;*.jpeg;*.png;*.bmp" };
        if (openFileDialog.ShowDialog() is true)
        {
            Image_FoodImage.Source = FoodImage = Utils.LoadImageToMemoryStream(
                openFileDialog.FileName
            );
            Button_AddFoodImage.Visibility = Visibility.Hidden;
        }
    }

    private void MenuItem_ChangeFoodImage_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog =
            new() { Title = "选择图片", Filter = $"图片|*.jpg;*.jpeg;*.png;*.bmp" };
        if (openFileDialog.ShowDialog() is true)
        {
            if (Image_FoodImage.Source is BitmapImage bitmapImage)
                bitmapImage.StreamSource.Close();
            Image_FoodImage.Source = FoodImage = Utils.LoadImageToMemoryStream(
                openFileDialog.FileName
            );
        }
    }

    private void Button_Cancel_Click(object sender, RoutedEventArgs e)
    {
        FoodImage?.StreamSource.Close();
        Close();
    }

    private void Button_Yes_Click(object sender, RoutedEventArgs e)
    {
        IsCancel = false;
        Close();
    }
}
