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

namespace VPet.Plugin.DemoClock
{
    /// <summary>
    /// winSetting.xaml 的交互逻辑
    /// </summary>
    public partial class winSetting : Window
    {
        private bool AllowChange = false;
        DemoClock Master;
        Setting Set;
        public winSetting(DemoClock master)
        {
            InitializeComponent();
            Master = master;
            Set = Master.Set;
            Switch24h.IsChecked = Set.Hour24;
            PlaceSilder.Value = Set.PlaceTop;
            OpacitySilder.Value = Set.Opacity * 100;
            SwitchAutoLayer.IsChecked = Set.PlaceAutoBack;
            NumTimeDiff.Value = Set.TimeShifting;
            NumDefCountDown.Value = Set.DefaultCountDown;
            NumTomatoWork.Value = Set.Tomato_WorkTime;
            NumTomatoRest.Value = Set.Tomato_RestTime;
            NumTomatoRestLong.Value = Set.Tomato_RestTimeLong;
            TextCountDown.Text = Set.CountDownVoice;
            TextTomatoWork.Text = Set.Tomato_WorkVoice;
            TextTomatoRest.Text = Set.Tomato_RestVoice;
            TextTomatoEnd.Text = Set.Tomato_EndVoice;

            AllowChange = true;
        }

        private void Switch24h_Checked(object sender, RoutedEventArgs e)
        {
            if (!AllowChange)
                return;
            Set.Hour24 = Switch24h.IsChecked.Value;
        }

        private void PlaceSilder_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!AllowChange)
                return;
            Set.PlaceTop = PlaceSilder.Value;
            Master.WPFTimeClock.Margin = new Thickness(0, Set.PlaceTop, 0, 0);
        }

        private void OpacitySilder_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!AllowChange)
                return;
            Set.Opacity = OpacitySilder.Value / 100;
            if(Master.WPFTimeClock.Opacity != 0.95)
                Master.WPFTimeClock.Opacity = Set.Opacity;
        }

        private void SwitchAutoLayer_Checked(object sender, RoutedEventArgs e)
        {
            if (!AllowChange)
                return;
            Set.PlaceAutoBack = SwitchAutoLayer.IsChecked.Value;
            if (Master.Set.PlaceAutoBack && Master.MW.Main.UIGrid_Back.Children.Contains(Master.WPFTimeClock))
            {
                Master.MW.Main.UIGrid_Back.Children.Remove(Master.WPFTimeClock);
                Master.MW.Main.UIGrid.Children.Insert(0, Master.WPFTimeClock);
            }
        }

        private void NumTimeDiff_ValueChanged(object sender, Panuon.WPF.SelectedValueChangedRoutedEventArgs<double?> e)
        {
            if (!AllowChange)
                return;
            Set.TimeShifting = NumTimeDiff.Value.Value;
        }

        private void NumDefCountDown_ValueChanged(object sender, Panuon.WPF.SelectedValueChangedRoutedEventArgs<double?> e)
        {
            if (!AllowChange)
                return;
            Set.DefaultCountDown = NumDefCountDown.Value.Value;
        }

        private void TextCountDown_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!AllowChange)
                return;
            Set.CountDownVoice = TextCountDown.Text;
        }

        private void NumTomatoWork_ValueChanged(object sender, Panuon.WPF.SelectedValueChangedRoutedEventArgs<double?> e)
        {
            if (!AllowChange)
                return;
            Set.Tomato_WorkTime = NumTomatoWork.Value.Value;
        }

        private void NumTomatoRest_ValueChanged(object sender, Panuon.WPF.SelectedValueChangedRoutedEventArgs<double?> e)
        {
            if (!AllowChange)
                return;
            Set.Tomato_RestTime = NumTomatoRest.Value.Value;
        }

        private void NumTomatoRestLong_ValueChanged(object sender, Panuon.WPF.SelectedValueChangedRoutedEventArgs<double?> e)
        {
            if (!AllowChange)
                return;
            Set.Tomato_RestTimeLong = NumTomatoRestLong.Value.Value;
        }
        private void TextTomatoWork_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!AllowChange)
                return;
            Set.Tomato_WorkVoice = TextTomatoWork.Text;
        }

        private void TextTomatoRest_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!AllowChange)
                return;
            Set.Tomato_RestVoice = TextTomatoRest.Text;
        }

        private void TextTomatoEnd_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!AllowChange)
                return;
            Set.Tomato_EndVoice = TextTomatoEnd.Text;
        }

        private void btn_path_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "音频文件|*.wav;*.mp3";

            if (openFileDialog.ShowDialog() != true)
            {
                return;
            }
            Button button = sender as Button;
            switch (button.Tag)
            {
                case "CountDown":
                    TextCountDown.Text = openFileDialog.FileName;
                    break;
                case "TomatoWork":
                    TextTomatoWork.Text = openFileDialog.FileName;
                    break;
                case "TomatoRest":
                    TextTomatoRest.Text = openFileDialog.FileName;
                    break;
                case "TomatoEnd":
                    TextTomatoEnd.Text = openFileDialog.FileName;
                    break;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Master.winSetting = null;
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Master.MW.ShowSetting(5);
        }

        //private void SwitchOn_Checked(object sender, RoutedEventArgs e)
        //{
        //    if (!AllowChange)
        //        return;
        //}
    }
}
