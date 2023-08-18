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
using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.ModMaker.ModEdit.FoodEdit;

/// <summary>
/// AddFoodWindow.xaml 的交互逻辑
/// </summary>
public partial class Window_AddFood : Window
{
    public bool IsCancel { get; internal set; } = true;

    public Window_AddFood()
    {
        InitializeComponent();
        Closed += (s, e) =>
        {
            if (IsCancel)
                if (Image_FoodImage.Source is BitmapImage image)
                    image.StreamSource.Close();
        };
    }

    private void Button_AddFoodImage_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog =
            new() { Title = "选择图片", Filter = $"图片|*.jpg;*.jpeg;*.png;*.bmp" };
        if (openFileDialog.ShowDialog() is true)
        {
            Image_FoodImage.Source = Utils.LoadImageToStream(openFileDialog.FileName);
            Button_AddFoodImage.Visibility = Visibility.Hidden;
        }
    }

    private void MenuItem_ChangeFoodImage_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog =
            new() { Title = "选择图片", Filter = $"图片|*.jpg;*.jpeg;*.png;*.bmp" };
        if (openFileDialog.ShowDialog() is true)
        {
            if (Image_FoodImage.Source is BitmapImage image)
                image.StreamSource.Close();
            Image_FoodImage.Source = Utils.LoadImageToStream(openFileDialog.FileName);
        }
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
