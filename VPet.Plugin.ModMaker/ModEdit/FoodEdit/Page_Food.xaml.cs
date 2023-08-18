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
using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.ModMaker.ModEdit.FoodEdit;

/// <summary>
/// Page_Food.xaml 的交互逻辑
/// </summary>
public partial class Page_Food : Page
{
    public ObservableCollection<Food> Foods { get; set; } = new();

    public Dictionary<string, Food> FoodDict { get; set; } = new();

    public Page_Food()
    {
        InitializeComponent();
        DataGrid_Food.ItemsSource = Foods;
        // TODO: 多语言
    }

    private void Button_AddFood_Click(object sender, RoutedEventArgs e)
    {
        var window = new Window_AddFood();
        window.Closed += (s, e) =>
        {
            if (s is not Window_AddFood addFoodWindow || addFoodWindow.IsCancel)
                return;
            var food = CreateFoodFromWindow(addFoodWindow);
            if (FoodDict.TryGetValue(food.Name, out var oldFood))
            {
                FoodDict[food.Name] = Foods[Foods.IndexOf(oldFood)] = food;
            }
            else
            {
                Foods.Add(food);
                FoodDict.Add(food.Name, food);
            }
        };
        ShowDialogX(window);
    }

    private void DataGrid_Food_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is not DataGrid dataGrid || dataGrid.SelectedItem is not Food oldFood)
            return;
        ChangeFoodInfo(oldFood);
    }

    private void TextBox_SearchFood_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox textBox)
            return;
        if (textBox.Text.Length > 0)
        {
            var newList = new ObservableCollection<Food>(
                Foods.Where(i => i.Name.Contains(textBox.Text))
            );
            if (newList.Count != Foods.Count)
                DataGrid_Food.ItemsSource = newList;
        }
        else
            DataGrid_Food.ItemsSource = Foods;
    }

    private void MenuItem_RemoveFood_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem || menuItem.Tag is not Food food)
            return;
        if (DataGrid_Food.ItemsSource is IList list && list.Count != Foods.Count)
            list.Remove(food);
        else
            Foods.Remove(food);
        FoodDict.Remove(food.Name);
    }

    private void MenuItem_ChangeFood_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem || menuItem.Tag is not Food food)
            return;
        ChangeFoodInfo(food);
    }
}
