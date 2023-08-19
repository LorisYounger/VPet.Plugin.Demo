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
using VPet.Plugin.ModMaker.ViewModels.ModEdit.FoodEdit;
using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.ModMaker.Views.ModEdit.FoodEdit;

/// <summary>
/// AddFoodWindow.xaml 的交互逻辑
/// </summary>
public partial class FoodEditWindow : Window
{
    public bool IsCancel { get; internal set; } = true;

    public FoodEditWindowVM ViewModel => (FoodEditWindowVM)DataContext;

    public FoodEditWindow()
    {
        InitializeComponent();
        DataContext = new FoodEditWindowVM();
        Closed += (s, e) =>
        {
            ViewModel.Close();
            if (IsCancel)
                ViewModel.Food.Value.Image.Value?.StreamSource?.Close();
        };
    }

    private void Button_Cancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Button_Yes_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(ViewModel.Food.Value.Id.Value))
        {
            MessageBox.Show("Id不可为空", "", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (ViewModel.Food.Value.CurrentI18nData.Value.Name.Value is null)
        {
            MessageBox.Show("名称不可为空", "", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (ViewModel.Food.Value.Image.Value is null)
        {
            MessageBox.Show("图像不可为空", "", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        IsCancel = false;
        Close();
    }
}
