using LinePutScript;
using Panuon.WPF;
using Panuon.WPF.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace VPet.Plugin.DemoClock
{
    /// <summary>
    /// ZoneInput.xaml 的交互逻辑
    /// </summary>
    public partial class ZoneInput : Window
    {
        LpsDocument lps;
        Setting _set;
        DemoClock _master;
        List<(string Name, int AdCode, int CityCode)> _data;
        List<string> _schBoxSource;

        public ZoneInput(DemoClock Master)
        {
            InitializeComponent();
            _set = Master.Set;
            _master = Master;
            // 使用异步加载资源
            LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                // 异步加载LPS文件数据，避免阻塞UI线程
                lps = await Task.Run(() => new LpsDocument(Properties.Resources.V3CityADCODE_lps));

                // 使用后台线程处理数据
                _data = await Task.Run(() => lps.Select(x => (x.Name, x.InfoToInt, x.TextToInt)).ToList());

                // 获取城市名并设置给下拉框数据源
                _schBoxSource = _data.Select(x => x.Name).ToList();
                // 在UI线程中更新ItemsSource
                Dispatcher.Invoke(() =>
                {
                    RegionBox.ItemsSource = _schBoxSource.Take(50);
                });
            }
            catch (Exception ex)
            {
                // 错误处理，显示异常信息
                MessageBoxX.Show($"加载数据失败: {ex.Message}");
            }
        }

        private void SchBox_SearchTextChanged(object sender, SearchTextChangedRoutedEventArgs e)
        {
            var searchBox = sender as SearchBox;
            var searchText = e.Text?.Trim()?.ToLower();

            // 异步更新搜索结果，避免阻塞UI线程
            Task.Run(() =>
            {
                var filteredData = string.IsNullOrEmpty(searchText)
                    ? _schBoxSource.Take(50).ToList()
                    : _schBoxSource.Where(x => x.Contains(searchText, StringComparison.OrdinalIgnoreCase)).Take(10).ToList();

                // 在UI线程中更新ItemsSource
                Dispatcher.Invoke(() =>
                {
                    searchBox.ItemsSource = filteredData;
                });
            });
        }

        private void SchBox_ItemClick(object sender, System.Windows.RoutedEventArgs e)
        {
            // 点击项目后的处理
            var searchBox = sender as SearchBox;
            var selectedText = searchBox.Text?.ToString();

            if (string.IsNullOrEmpty(selectedText)) return;

            // 更新 AdCode
            var selectedItem = _data.FirstOrDefault(x => x.Name == selectedText);
            if (selectedItem != default)
            {
                _set.AdCode = selectedItem.AdCode;
            }
            _master.HandleWeatherAsync();
        }
    }
}
