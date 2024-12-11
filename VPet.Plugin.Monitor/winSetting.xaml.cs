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

namespace VPet.Plugin.Monitor
{
    /// <summary>
    /// winSetting.xaml 的交互逻辑
    /// </summary>
    public partial class winSetting : Window
    {
        Setting Set;
        MonitorMain master;
        public bool IsAllow=false;
        public winSetting(MonitorMain master1)
        {
            InitializeComponent();
            master = master1;
            Set = master.Set;
            PlaceSilder_y.Value = Set.PlaceTop;
            Opacity.Value = Set.WindowOpacity;
            Switch_y.IsChecked = Set.isverticalshow;
            Switch_x.IsChecked = Set.ishorizonalshow;
            Switch_Gpu.IsChecked = Set.IsGPUWoring;
            Switch_z.IsChecked = !Set.isfrond;
            Switch_Color.IsChecked = Set.IsColorful;
            List_Net.ItemsSource= master.MB.NetIntances;
            List_Net.SelectedIndex=master.MB.NetIntances.IndexOf(Set.NetSelected);
            List<string> GPUNAMES= new List<string>();
            for (int i = 0; i < master.MB.GPUIntances.Count; i++)
            {
                GPUNAMES.Add($"phys_{i}");
            }
            List_GPU.ItemsSource = GPUNAMES;
            List_GPU.SelectedIndex = Set.GPUSelected;
            IsAllow = true;

        }

        private void PlaceSilder_y_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!IsAllow)
                return;
            Set.PlaceTop=PlaceSilder_y.Value;
            master.MB.Margin=new Thickness(0,Set.PlaceTop,0,0);

        }

        private void Opacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!IsAllow)
                return;
            Set.WindowOpacity = Opacity.Value;
            master.MB.Opacity = Opacity.Value;
        }

        private void Switch_y_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsAllow)
                return;
            Set.IsVerticalShow = Switch_y.IsChecked.Value;
            if (Switch_y.IsChecked.Value)
            {
                master.MB.Separator_vertical.Visibility = Visibility.Visible;
            }
            else
            {
                master.MB.Separator_vertical.Visibility = Visibility.Hidden;
            }
        }

        private void Switch_x_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsAllow)
                return;
            Set.IsHorizonalShow = Switch_x.IsChecked.Value;
            if (Switch_x.IsChecked.Value)
            {
                master.MB.Separator_horizonal.Visibility = Visibility.Visible;
            }
            else
            {
                master.MB.Separator_horizonal.Visibility = Visibility.Hidden;
            }
        }


        private void Switch_Gpu_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsAllow)
                return;
            Set.IsGPUWoring = Switch_Gpu.IsChecked.Value;
        }


        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            master.MW.ShowSetting(5);
        }

        private void List_Net_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsAllow)
                return;
            Set.NetSelected = List_Net.SelectedItem.ToString();
            master.MB.Netinit(Set.NetSelected);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            master.winSetting = null;
        }

        private void Switch_z_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsAllow)
                return;
            master.Set.IsFrond = !Switch_z.IsChecked.Value;
            if (master.MW.Main.UIGrid.Children.Contains(master.MB)&&!Set.isfrond)
            {
                master.MW.Main.UIGrid.Children.Remove(master.MB);
                master.MW.Main.UIGrid_Back.Children.Insert(0, master.MB);
            }
            else if (master.MW.Main.UIGrid_Back.Children.Contains(master.MB) && Set.isfrond)
            {
                master.MW.Main.UIGrid_Back.Children.Remove(master.MB);
                master.MW.Main.UIGrid.Children.Insert(0, master.MB);
            }
        }

        private void List_GPU_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsAllow)
                return;
            Set.GPUSelected = List_GPU.SelectedIndex;
            master.MB.GPUGetIntances();
            master.MB.GPUinit(Set.GPUSelected);
        }

        private void Switch_Color_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsAllow)
                return;
            Set.IsColorful = Switch_Color.IsChecked.Value;
            if (!Set.IsColorful)
            {
                master.MB.Bar_CPU.Dispatcher.BeginInvoke(new Action(() => { master.MB.Bar_CPU.SetResourceReference(Border.BackgroundProperty, "SuccessProgressBarForeground"); }));
                master.MB.Bar_GPU.Dispatcher.BeginInvoke(new Action(() => { master.MB.Bar_GPU.SetResourceReference(Border.BackgroundProperty, "SuccessProgressBarForeground"); }));
                master.MB.Bar_RAM.Dispatcher.BeginInvoke(new Action(() => { master.MB.Bar_RAM.SetResourceReference(Border.BackgroundProperty, "SuccessProgressBarForeground"); }));
                master.MB.Bar_Net.Dispatcher.BeginInvoke(new Action(() => { master.MB.Bar_Net.SetResourceReference(Border.BackgroundProperty, "SuccessProgressBarForeground"); }));
            }
        }
    }
}
