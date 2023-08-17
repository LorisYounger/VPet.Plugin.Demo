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
using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.ModMaker.WinModEdit;

/// <summary>
/// winModInfo.xaml 的交互逻辑
/// </summary>
public partial class ModEditWindow : Window
{
    public string ModName { get; set; } = string.Empty;
    public ObservableCollection<Food> Foods { get; set; } = new();

    public Dictionary<string, Food> FoodDict { get; set; } = new();

    public ModEditWindow()
    {
        InitializeComponent();
        DataGrid_Food.ItemsSource = Foods;
        Closed += WinModInfo_Closed;
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

    private void Button_AddFood_Click(object sender, RoutedEventArgs e)
    {
        this.IsEnabled = false;
        var window = new AddFoodWindow();
        window.Closed += (s, e) =>
        {
            if (s is not AddFoodWindow addFoodWindow)
                return;
            if (addFoodWindow.IsCancel)
            {
                this.IsEnabled = true;
                return;
            }
            var food = CreateFoodFormWindow(addFoodWindow);
            if (FoodDict.TryGetValue(food.Name, out var oldFood))
            {
                FoodDict[food.Name] = Foods[Foods.IndexOf(oldFood)] = food;
            }
            else
            {
                Foods.Add(food);
                FoodDict.Add(food.Name, food);
            }
            this.IsEnabled = true;
        };
        window.Show();
    }

    private void Button_AddItem_Click(object sender, RoutedEventArgs e) { }

    private void Button_AddAnime_Click(object sender, RoutedEventArgs e) { }

    private void Button_AddAudio_Click(object sender, RoutedEventArgs e) { }

    private void Button_AddClickText_Click(object sender, RoutedEventArgs e) { }

    private void Button_AddLowText_Click(object sender, RoutedEventArgs e) { }

    private void DataGrid_Food_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is not DataGrid dataGrid || dataGrid.SelectedItem is not Food oldFood)
            return;
        this.IsEnabled = false;
        var window = new AddFoodWindow();
        window.TextBox_FoodName.Text = oldFood.Name;
        window.Image_FoodImage.Source = oldFood.ImageSource;
        window.TextBox_FoodDescription.Text = oldFood.Desc;
        foreach (ComboBoxItem item in window.ComboBox_FoodType.Items)
            if (item.Tag.ToString() == oldFood.Type.ToString())
                window.ComboBox_FoodType.SelectedItem = item;
        window.TextBox_Strength.Text = oldFood.Strength.ToString();
        window.TextBox_StrengthFood.Text = oldFood.StrengthFood.ToString();
        window.TextBox_StrengthDrink.Text = oldFood.StrengthDrink.ToString();
        window.TextBox_Health.Text = oldFood.Health.ToString();
        window.TextBox_Feeling.Text = oldFood.Feeling.ToString();
        window.TextBox_Likability.Text = oldFood.Likability.ToString();
        window.TextBox_Price.Text = oldFood.Price.ToString();
        window.TextBox_Exp.Text = oldFood.Exp.ToString();
        window.Button_AddFoodImage.Visibility = Visibility.Hidden;
        window.Closed += (s, e) =>
        {
            if (s is not AddFoodWindow addFoodWindow)
                return;
            if (addFoodWindow.IsCancel)
            {
                this.IsEnabled = true;
                return;
            }
            var food = CreateFoodFormWindow(addFoodWindow);
            if (FoodDict.TryGetValue(food.Name, out var tempFood))
            {
                FoodDict[food.Name] = Foods[Foods.IndexOf(tempFood)] = food;
            }
            else
            {
                Foods[Foods.IndexOf(oldFood)] = food;
                FoodDict.Remove(oldFood.Name);
                FoodDict.Add(food.Name, food);
            }
            this.IsEnabled = true;
        };
        window.Show();
    }

    private void MenuItem_RemoveFood_Click(object sender, RoutedEventArgs e) { }
}
