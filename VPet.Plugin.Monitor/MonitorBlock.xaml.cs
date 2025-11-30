using LinePutScript.Localization.WPF;
using Panuon.WPF.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO.Packaging;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading;
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

namespace VPet.Plugin.Monitor
{
    /// <summary>
    /// MonitorBlock.xaml 的交互逻辑
    /// </summary>
    public partial class MonitorBlock : UserControl
    {
        private MonitorMain master;
        private float MaxByteps = 0;
        public Timer DefaultWorker;
        public Timer GPUWorker;
        public float MaxNet = 0;
        public PerformanceCounter CpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        public PerformanceCounter RamAVACounter = new PerformanceCounter("Memory", "Available Bytes");
        public PerformanceCounter RamCLCounter = new PerformanceCounter("Memory", "Commit Limit");

        public List<string> NetIntances = new List<string>();
        public List<List<string>> GPUIntances = new List<List<string>>();
        public PerformanceCounter NetCounter;
        public PerformanceCounterCategory Founder_GPU = new PerformanceCounterCategory("GPU Engine");
        public PerformanceCounterCategory Founder_Net = new PerformanceCounterCategory("Network Interface");
        public List<PerformanceCounter> GpuEnginelist = new List<PerformanceCounter>();
        public string[] GPUintancenames;
        public MonitorBlock(MonitorMain master1)
        {
            InitializeComponent();
            master = master1;
            master.MW.Main.UIGrid.Children.Insert(0, this);
            Margin = new Thickness(0, master.Set.PlaceTop, 0, 0);
            Opacity = master.Set.WindowOpacity;
            if (master.Set.isverticalshow)
            {
                Separator_vertical.Visibility = Visibility.Visible;
            }
            else
            {
                Separator_vertical.Visibility = Visibility.Hidden;
            }
            if (master.Set.ishorizonalshow)
            {
                Separator_horizonal.Visibility = Visibility.Visible;
            }
            else
            {
                Separator_horizonal.Visibility = Visibility.Hidden;
            }
            if (master.MW.Main.UIGrid.Children.Contains(this) && !master.Set.isfrond)
            {
                master.MW.Main.UIGrid.Children.Remove(this);
                master.MW.Main.UIGrid_Back.Children.Insert(0, this);
            }
            else if (master.MW.Main.UIGrid_Back.Children.Contains(this) && master.Set.isfrond)
            {
                master.MW.Main.UIGrid_Back.Children.Remove(this);
                master.MW.Main.UIGrid.Children.Insert(0, this);
            }
            master.MW.Main.MouseEnter += UserControl_MouseEnter;
            master.MW.Main.MouseLeave += UserControl_MouseLeave;
            StartWork();
            DefaultWorker = new Timer(DefaultWorking, DefaultWorker, 0, 1000);
            GPUWorker = new Timer(GPUWorking, GPUWorker, 0, 3000);
        }
        public void GPUWorking(object o)
        {
            if (master.Set.IsGPUWoring)
            {
                try
                {
                    if (GPUintancenames.Count() != Founder_GPU.GetInstanceNames().Count())
                    {
                        GPUGetIntances();
                        GPUinit(master.Set.GPUSelected);
                    }
                    float G = GPUCounterValue();
                    ChangeUIText(Using_GPU, "GPU", G);
                    MoveProcessBar(Bar_GPU, (double)G);
                }
                catch(Exception e)
                {
                    master.Set.IsGPUWoring = false;
                    Dispatcher.Invoke(() =>
                    {
                        MessageBoxX.Show("数据查找失败,已自动关闭GPU监控，\n错误信息:\n{0}\n错误堆栈:\n{1}".Translate(e.Message, e.StackTrace), "性能监视器报错".Translate(), icon: MessageBoxIcon.Error);
                    });
                }
            }
            else
            {
                master.MB.Using_GPU.Dispatcher.BeginInvoke(new Action(() => { master.MB.Using_GPU.Text = "GPU:O.o"; }));
                master.MB.Bar_GPU.Dispatcher.BeginInvoke(new Action(() => { master.MB.Bar_GPU.Width = 67.25; }));
            }
        }
        public void DefaultWorking(object o)
        {
            try
            {
                float C = CpuCounter.NextValue();
                float CL = RamCLCounter.NextValue();
                float AVA = RamAVACounter.NextValue();
                float R = (CL - AVA) / CL * 100;
                ChangeUIText(Using_CPU, "CPU", C);
                ChangeUIText(Using_RAM, "RAM", R);
                MoveProcessBar(Bar_CPU, (double)C);
                MoveProcessBar(Bar_RAM, (double)R);
                float N = NetCounter.NextValue();
                ChangeUIText(Using_Net, $"↑↓:{ReadNetByte(N)}");
                MoveProcessBar(Bar_Net, N);
            }
            catch (Exception e)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBoxX.Show("数据查找失败,错误信息:\n{0}\n错误堆栈:\n{1}".Translate(e.Message, e.StackTrace), "性能监视器报错".Translate(), icon: MessageBoxIcon.Error);
                });
            }
        }
        public void StartWork()
        {
            if (master.Set.NetSelected == string.Empty)
            {
                if(NetGetIntances() != -1)
                    Netinit(NetIntances[NetGetIntances()]);
            }
            else
            {
                try
                {
                    NetGetIntances();
                    Netinit(master.Set.NetSelected);
                }
                catch
                {
                    if(NetGetIntances() != -1)
                        Netinit(NetIntances[NetGetIntances()]);
                }
            }
            GPUGetIntances();
            GPUinit(master.Set.GPUSelected);

        }
        public int NetGetIntances()
        {
            try
            {
                NetIntances.Clear();
                NetIntances.AddRange(Founder_Net.GetInstanceNames());
                int NetIndex = 0;
                foreach (string intancename in NetIntances)
                {
                    if (intancename.Contains("WIFI"))
                    {
                        NetIndex = NetIntances.IndexOf(intancename);
                    }
                }
                return NetIndex;
            }
            catch(Exception e)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBoxX.Show("信息查找失败,错误信息:\n{0}\n错误堆栈:\n{1}".Translate(e.Message, e.StackTrace), "性能监视器报错".Translate(), icon: MessageBoxIcon.Error);
                });
                return -1;
            }
        }
        public void GPUinit(int CounterIndex)
        {
            GpuEnginelist.Clear();
            foreach (string name in GPUIntances[CounterIndex])
            {
                GpuEnginelist.Add(new PerformanceCounter("GPU Engine", "Utilization Percentage", name));
            }
        }
        public float GPUCounterValue()
        {
            float persentage = 0;
            foreach (PerformanceCounter counter in GpuEnginelist)
            {
                persentage += counter.NextValue();
            }
            return persentage;
        }
        public void Netinit(string CounterName)
        {
            NetCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", CounterName);
        }
        public void GPUGetIntances()
        {
            try
            {
                GPUintancenames = Founder_GPU.GetInstanceNames();
                GPUIntances.Clear();
            }
            catch (Exception e)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBoxX.Show("信息查找失败,错误信息:\n{0}\n错误堆栈:\n{1}".Translate(e.Message, e.StackTrace), "性能监视器报错".Translate(), icon: MessageBoxIcon.Error);
                });
                return;
            }
            for (int i = 0; i <= 5; i++)
            {
                GPUIntances.Add(new List<string>());
                foreach (string j in GPUintancenames)
                {
                    if (j.Contains($"phys_{i}"))
                    {
                        GPUIntances[i].Add(j);
                    }
                }
                if (GPUIntances[i].Count == 0)
                {
                    GPUIntances.Remove(GPUIntances[i]);
                    break;
                }
            }
        }

        public void MoveProcessBar(Border Foreground, double percentage)
        {
            if (master.Set.IsColorful)
            {
                if (percentage <= 33)
                {
                    Foreground.Dispatcher.BeginInvoke(new Action(() => { Foreground.SetResourceReference(Border.BackgroundProperty, "SuccessProgressBarForeground"); }));
                }
                else if (percentage < 66)
                {
                    Foreground.Dispatcher.BeginInvoke(new Action(() => { Foreground.SetResourceReference(Border.BackgroundProperty, "WarningProgressBarForeground"); }));
                }
                else
                {
                    Foreground.Dispatcher.BeginInvoke(new Action(() => { Foreground.SetResourceReference(Border.BackgroundProperty, "DangerProgressBarForeground"); }));
                }
            }
            Foreground.Dispatcher.BeginInvoke(new Action(() => { Foreground.Width = 134.5 * percentage / 100; }));
        }
        public void MoveProcessBar(Border Foreground, float bytenums)
        {
            double persentagenet = (double)(bytenums / MaxByteps);
            if (master.Set.IsColorful)
            {
                if (persentagenet <= 0.33)
                {
                    Foreground.Dispatcher.BeginInvoke(new Action(() => { Foreground.SetResourceReference(Border.BackgroundProperty, "SuccessProgressBarForeground"); }));
                }
                else if (persentagenet < 0.66)
                {
                    Foreground.Dispatcher.BeginInvoke(new Action(() => { Foreground.SetResourceReference(Border.BackgroundProperty, "WarningProgressBarForeground"); }));
                }
                else
                {
                    Foreground.Dispatcher.BeginInvoke(new Action(() => { Foreground.SetResourceReference(Border.BackgroundProperty, "DangerProgressBarForeground"); }));
                }
            }
            Foreground.Dispatcher.BeginInvoke(new Action(() => { Foreground.Width = 134.5 * persentagenet; }));
        }

        public void ChangeUIText(TextBlock textBlock, string content)
        {
            textBlock.Dispatcher.BeginInvoke((Action)(() => { textBlock.Text = content; }));
        }
        public void ChangeUIText(TextBlock textBlock, string name, float content)
        {
            textBlock.Dispatcher.BeginInvoke((Action)(() => { textBlock.Text = $"{name}: {content.ToString("0.0")}%"; }));
        }

        private string ReadNetByte(float bytenum)
        {
            if (bytenum > MaxByteps)
            {
                MaxByteps = bytenum;
            }
            if (bytenum < 1000)
            {
                return (bytenum).ToString("0.0") + "B";
            }
            else if (bytenum >= 1000 && bytenum <= 1000000)
            {
                return (bytenum / 1024).ToString("0.0") + "KB";
            }
            else if ((bytenum >= 1000000))
            {
                return (bytenum / 1048576).ToString("0.0") + "MB";
            }
            else if ((bytenum >= 1000000000))
            {
                return (bytenum / 1073741824).ToString("0.0") + "GB";
            }
            return "o.O";
        }

        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            master.Setting();
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {

            if (master.MW.Main.UIGrid_Back.Children.Contains(this) && !master.Set.isfrond)
            {
                master.MW.Main.UIGrid_Back.Children.Remove(this);
                master.MW.Main.UIGrid.Children.Insert(0, this);
            }
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            if (master.MW.Main.UIGrid.Children.Contains(this) && !master.Set.isfrond)
            {
                master.MW.Main.UIGrid.Children.Remove(this);
                master.MW.Main.UIGrid_Back.Children.Insert(0, this);
            }
        }
    }
}
