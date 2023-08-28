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
using VPet.Plugin.ModMaker.ViewModels.ModEdit.FoodEdit;
using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.ModMaker.Views.ModEdit.FoodEdit;

/// <summary>
/// Page_Food.xaml 的交互逻辑
/// </summary>
public partial class FoodPage : Page
{
    public FoodPageVM ViewModel => (FoodPageVM)DataContext;

    public FoodPage()
    {
        InitializeComponent();
        DataContext = new FoodPageVM();
    }

    private void DataGrid_Food_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is not DataGrid dataGrid || dataGrid.SelectedItem is not FoodModel food)
            return;
        ViewModel.EditFood(food);
    }
}
