using LinePutScript.Localization.WPF;
using Panuon.WPF.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace VPet.Plugin.DoingDisplay
{
    /// <summary>
    /// winSetting.xaml 的交互逻辑
    /// </summary>
    public partial class winSetting : Window
    {
        private readonly DoingDisplay plugin;
        private SortMode _sortMode = SortMode.LastUse;
        private bool _sortAscending = false;

        public winSetting(DoingDisplay plugin)
        {
            InitializeComponent();
            Resources = Application.Current.Resources;
            this.plugin = plugin;
            RefreshData();
        }

        private List<SoftwareStatisticalVM> BuildViewModels()
        {
            var today = DateTime.Now.Date;
            var last7Days = Enumerable.Range(0, 7)
                .Select(i => today.AddDays(-6 + i))
                .ToList();
            var todayDaily = plugin.Set.Dailies.Find(d => d.Date == today);

            var vms = plugin.Set.Statisticals.Select(stat =>
            {
                var dailyStats = last7Days.Select(date =>
                {
                    var daily = plugin.Set.Dailies.Find(d => d.Date == date);
                    int minutes = daily?.SoftwareUsage.GetValueOrDefault(stat.SoftWareProcessName, 0) ?? 0;
                    return new DayUsage
                    {
                        Day = date.ToString("ddd"),
                        DateStr = date.ToString("MM/dd"),
                        Minute = minutes
                    };
                }).ToList();

                int maxMin = dailyStats.Max(d => d.Minute);
                foreach (var day in dailyStats)
                    day.BarHeight = maxMin > 0
                        ? (day.Minute > 0 ? Math.Max(2, day.Minute * 36.0 / maxMin) : 0)
                        : 0;

                int todayMin = todayDaily?.SoftwareUsage.GetValueOrDefault(stat.SoftWareProcessName, 0) ?? 0;
                return new SoftwareStatisticalVM(stat, dailyStats, todayMin);
            });

            return (_sortMode switch
            {
                SortMode.Name        => _sortAscending ? vms.OrderBy(v => v.SoftWare)      : vms.OrderByDescending(v => v.SoftWare),
                SortMode.TodayMinute => _sortAscending ? vms.OrderBy(v => v.TodayMinute)   : vms.OrderByDescending(v => v.TodayMinute),
                SortMode.TotalMinute => _sortAscending ? vms.OrderBy(v => v.TotalMinute)   : vms.OrderByDescending(v => v.TotalMinute),
                _                    => _sortAscending ? vms.OrderBy(v => v.LastUse)       : vms.OrderByDescending(v => v.LastUse),
            }).ToList();
        }

        private List<HistoryEntryVM> BuildDailyHistory()
        {
            return plugin.Set.Dailies
                .OrderByDescending(d => d.Date)
                .Select(d =>
                {
                    var items = d.SoftwareUsage
                        .Select(kv => new SoftwareUsageItem
                        {
                            Name = plugin.Set.Statisticals.Find(s => s.SoftWareProcessName == kv.Key)?.SoftWare ?? kv.Key,
                            Minute = kv.Value
                        })
                        .OrderByDescending(s => s.Minute)
                        .ToList();
                    int maxMin = items.Count > 0 ? items[0].Minute : 0;
                    foreach (var s in items)
                        s.Fraction = maxMin > 0 ? (double)s.Minute / maxMin * 100 : 0;
                    return new HistoryEntryVM
                    {
                        Header = d.Date.ToString("yyyy-MM-dd"),
                        SubHeader = d.Date.ToString("ddd"),
                        Minute = d.Minute,
                        TopSoftware = items
                    };
                }).ToList();
        }

        private List<HistoryEntryVM> BuildMonthlyHistory()
        {
            return plugin.Set.Dailies
                .GroupBy(d => new DateTime(d.Date.Year, d.Date.Month, 1))
                .OrderByDescending(g => g.Key)
                .Select(g =>
                {
                    var dict = new Dictionary<string, int>();
                    foreach (var day in g)
                        foreach (var kv in day.SoftwareUsage)
                        {
                            dict.TryGetValue(kv.Key, out int existing);
                            dict[kv.Key] = existing + kv.Value;
                        }
                    var items = dict
                        .Select(kv => new SoftwareUsageItem
                        {
                            Name = plugin.Set.Statisticals.Find(s => s.SoftWareProcessName == kv.Key)?.SoftWare ?? kv.Key,
                            Minute = kv.Value
                        })
                        .OrderByDescending(s => s.Minute)
                        .ToList();
                    int maxMin = items.Count > 0 ? items[0].Minute : 0;
                    foreach (var s in items)
                        s.Fraction = maxMin > 0 ? (double)s.Minute / maxMin * 100 : 0;
                    return new HistoryEntryVM
                    {
                        Header = $"{g.Key.Year}年{g.Key.Month:D2}月",
                        SubHeader = string.Empty,
                        Minute = g.Sum(d => d.Minute),
                        TopSoftware = items
                    };
                }).ToList();
        }

        private void RefreshData()
        {
            icStatisticals.ItemsSource = BuildViewModels();
            RefreshHistory();
        }

        private void RefreshHistory()
        {
            if (icHistory == null) return;
            icHistory.ItemsSource = rbDaily?.IsChecked == true
                ? BuildDailyHistory()
                : BuildMonthlyHistory();
        }

        private void RbDaily_Checked(object sender, RoutedEventArgs e) => RefreshHistory();
        private void RbMonthly_Checked(object sender, RoutedEventArgs e) => RefreshHistory();

        private void RbSort_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && Enum.TryParse<SortMode>(fe.Tag?.ToString(), out var mode))
            {
                _sortMode = mode;
                if (icStatisticals != null)
                    icStatisticals.ItemsSource = BuildViewModels();
            }
        }

        private void BtnSortDir_Click(object sender, RoutedEventArgs e)
        {
            _sortAscending = !_sortAscending;
            btnSortDir.Content = _sortAscending ? "↑" : "↓";
            icStatisticals.ItemsSource = BuildViewModels();
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBoxX.Show("确定要清除统计数据吗？".Translate(), "清除前确认".Translate(), MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                plugin.Set.Statisticals.Clear();
                plugin.Set.Dailies.Clear();
                RefreshData();
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            RefreshData();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            plugin.winSetting = null;
        }
    }

    public class DayUsage
    {
        public string Day { get; set; } = string.Empty;
        public string DateStr { get; set; } = string.Empty;
        public int Minute { get; set; }
        public double BarHeight { get; set; }
        public string DurationText => Minute > 0
            ? (Minute >= 60 ? $"{Minute / 60}h {Minute % 60}m" : $"{Minute}m")
            : string.Empty;
    }

    public class SoftwareStatisticalVM : INotifyPropertyChanged
    {
        private readonly Setting.SoftwareStatistical _stat;

        public SoftwareStatisticalVM(Setting.SoftwareStatistical stat, List<DayUsage> dailyStats, int todayMinute)
        {
            _stat = stat;
            DailyStats = dailyStats;
            TodayMinute = todayMinute;
        }

        public string SoftWare
        {
            get => _stat.SoftWare;
            set
            {
                if (_stat.SoftWare != value)
                {
                    _stat.SoftWare = value;
                    _stat.isEdit = true;
                    OnPropertyChanged();
                }
            }
        }

        public string SoftWareProcessName => _stat.SoftWareProcessName;
        /// <summary>
        /// 格式化后的使用时长, 用于显示
        /// </summary>
        public string DurationText => _stat.Minute >= 60 ? $"{_stat.Minute / 60}h {_stat.Minute % 60}m" : $"{_stat.Minute}m";
        public DateTime LastUse => _stat.LastUse;
        public int TodayMinute { get; }
        public int TotalMinute => _stat.Minute;
        public List<DayUsage> DailyStats { get; }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class SoftwareUsageItem
    {
        public string Name { get; set; } = string.Empty;
        public int Minute { get; set; }
        public double Fraction { get; set; }
        public string DurationText => Minute >= 60 ? $"{Minute / 60}h {Minute % 60}m" : $"{Minute}m";
    }

    public class HistoryEntryVM
    {
        public string Header { get; set; } = string.Empty;
        public string SubHeader { get; set; } = string.Empty;
        public int Minute { get; set; }
        public string DurationText => Minute >= 60 ? $"{Minute / 60}h {Minute % 60}m" : $"{Minute}m";
        public List<SoftwareUsageItem> TopSoftware { get; set; } = new();
    }

    public enum SortMode { LastUse, Name, TodayMinute, TotalMinute }
}
