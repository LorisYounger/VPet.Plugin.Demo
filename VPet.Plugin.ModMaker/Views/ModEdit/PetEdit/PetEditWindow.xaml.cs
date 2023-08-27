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
using VPet.Plugin.ModMaker.ViewModels.ModEdit.PetEdit;

namespace VPet.Plugin.ModMaker.Views.ModEdit.PetEdit;

/// <summary>
/// PetEditWindow.xaml 的交互逻辑
/// </summary>
public partial class PetEditWindow : Window
{
    public PetEditWindowVM ViewModel => (PetEditWindowVM)DataContext;

    public PetEditWindow()
    {
        DataContext = new PetEditWindowVM();
        InitializeComponent();
    }
}
