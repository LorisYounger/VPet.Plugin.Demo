using LinePutScript.Localization.WPF;
using Panuon.WPF.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace VPet.Plugin.DoingDisplay
{
    public partial class winSetting : Window
    {
        private readonly DoingDisplay plugin;
        private SortMode _sortMode = SortMode.LastUse;
        private bool _sortAscending;
        private DateTime statisticsDate;
        private readonly Setting.DailyStatistical?[] recentDays = new Setting.DailyStatistical?[7];
        private Dictionary<string, Setting.SoftwareStatistical> historySoftware = new();

        public winSetting(DoingDisplay plugin)
        {
            this.plugin = plugin;
            InitializeComponent();
            Resources.MergedDictionaries.Add(Application.Current.Resources);
            RefreshData();
            ShowBox.IsChecked = plugin.Set.ShowDoing;
        }

        private void RefreshData()
        {
            statisticsDate = DateTime.Today;
            for (int i = 0; i < recentDays.Length; i++)
            {
                var date = statisticsDate.AddDays(i - 6);
                recentDays[i] = plugin.Set.Dailies.Find(d => d.Date == date);
            }

            var items = new List<SoftwareStatisticalVM>();
            for (int i = 0; i < plugin.Set.Statisticals.Count; i++)
            {
                var stat = plugin.Set.Statisticals[i];
                int todayMinute = recentDays[6]?.SoftwareUsage.GetValueOrDefault(stat.SoftWareProcessName, 0) ?? 0;
                items.Add(new SoftwareStatisticalVM(stat, todayMinute));
            }
            icStatisticals.ItemsSource = items;
            ApplySort();
            icHistory.ItemsSource = null;
            historySoftware.Clear();
            if (tabs.SelectedIndex == 1)
                RefreshHistory();
            else
                UpdateExpandAllButton();
        }

        private void ApplySort()
        {
            var view = CollectionViewSource.GetDefaultView(icStatisticals.ItemsSource);
            string property = _sortMode switch
            {
                SortMode.Name => nameof(SoftwareStatisticalVM.SoftWare),
                SortMode.TodayMinute => nameof(SoftwareStatisticalVM.TodayMinute),
                SortMode.TotalMinute => nameof(SoftwareStatisticalVM.TotalMinute),
                _ => nameof(SoftwareStatisticalVM.LastUse),
            };
            using (view.DeferRefresh())
            {
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription(property,
                    _sortAscending ? ListSortDirection.Ascending : ListSortDirection.Descending));
            }
        }

        private void RefreshHistory()
        {
            historySoftware.Clear();
            for (int i = 0; i < plugin.Set.Statisticals.Count; i++)
            {
                var stat = plugin.Set.Statisticals[i];
                historySoftware[stat.SoftWareProcessName] = stat;
            }

            var entries = new List<HistoryEntryVM>();
            var months = new Dictionary<DateTime, HistoryEntryVM>();
            for (int i = 0; i < plugin.Set.Dailies.Count; i++)
            {
                var day = plugin.Set.Dailies[i];
                if (rbDaily.IsChecked == true)
                {
                    entries.Add(new HistoryEntryVM
                    {
                        Date = day.Date,
                        Header = day.Date.ToString("yyyy-MM-dd"),
                        SubHeader = day.Date.ToString("ddd"),
                        Minute = day.Minute,
                        Days = new List<Setting.DailyStatistical> { day }
                    });
                }
                else
                {
                    var month = new DateTime(day.Date.Year, day.Date.Month, 1);
                    if (!months.TryGetValue(month, out var entry))
                    {
                        entry = new HistoryEntryVM { Date = month, Header = $"{month.Year}年{month.Month:D2}月" };
                        months.Add(month, entry);
                        entries.Add(entry);
                    }
                    entry.Minute += day.Minute;
                    entry.Days.Add(day);
                }
            }
            icHistory.ItemsSource = entries.OrderByDescending(e => e.Date).ToList();
            UpdateExpandAllButton();
        }

        private void Details_Loaded(object sender, RoutedEventArgs e) => RefreshDetails((ItemsControl)sender);

        private void Details_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var control = (ItemsControl)sender;
            if (control.IsLoaded)
                RefreshDetails(control);
        }

        private void RefreshDetails(ItemsControl control)
        {
            if (control.DataContext is SoftwareStatisticalVM stat)
            {
                var days = new List<DayUsage>();
                for (int i = 0; i < recentDays.Length; i++)
                {
                    var date = statisticsDate.AddDays(i - 6);
                    days.Add(new DayUsage
                    {
                        Day = date.ToString("ddd"),
                        DateStr = date.ToString("MM/dd"),
                        Minute = recentDays[i]?.SoftwareUsage.GetValueOrDefault(stat.SoftWareProcessName, 0) ?? 0
                    });
                }
                int maxMinute = days.Max(d => d.Minute);
                foreach (var day in days)
                    day.BarHeight = maxMinute > 0 && day.Minute > 0 ? Math.Max(2, day.Minute * 36.0 / maxMinute) : 0;
                control.ItemsSource = days;
            }
            else if (control.DataContext is HistoryEntryVM entry)
            {
                var usage = new Dictionary<string, int>();
                foreach (var day in entry.Days)
                    foreach (var item in day.SoftwareUsage)
                    {
                        usage.TryGetValue(item.Key, out int minute);
                        usage[item.Key] = minute + item.Value;
                    }
                var items = usage.Select(item => new SoftwareUsageItem
                {
                    Name = historySoftware.TryGetValue(item.Key, out var software) ? software.SoftWare : item.Key,
                    Minute = item.Value
                }).OrderByDescending(item => item.Minute).ToList();
                int maxMinute = items.Count > 0 ? items[0].Minute : 0;
                foreach (var item in items)
                    item.Fraction = maxMinute > 0 ? item.Minute * 100.0 / maxMinute : 0;
                control.ItemsSource = items;
            }
        }

        private void Tabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded && e.Source == tabs)
            {
                if (tabs.SelectedIndex == 1)
                    RefreshHistory();
                else
                    UpdateExpandAllButton();
            }
        }

        private void HistoryMode_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && tabs.SelectedIndex == 1)
                RefreshHistory();
        }

        private void RbSort_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                _sortMode = Enum.Parse<SortMode>((string)((RadioButton)sender).Tag);
                ApplySort();
            }
        }

        private void BtnSortDir_Click(object sender, RoutedEventArgs e)
        {
            _sortAscending = !_sortAscending;
            btnSortDir.Content = _sortAscending ? "↑" : "↓";
            ApplySort();
        }

        private void BtnExpandAll_Click(object sender, RoutedEventArgs e)
        {
            bool expanded = !AllDetailsExpanded;
            if (tabs.SelectedIndex == 0)
            {
                foreach (SoftwareStatisticalVM item in icStatisticals.Items)
                    item.IsExpanded = expanded;
            }
            else
            {
                foreach (HistoryEntryVM item in icHistory.Items)
                    item.IsExpanded = expanded;
            }
            UpdateExpandAllButton();
        }

        private void DetailToggle_Click(object sender, RoutedEventArgs e) => UpdateExpandAllButton();

        private bool AllDetailsExpanded => tabs.SelectedIndex == 0
            ? icStatisticals.Items.Count > 0 && icStatisticals.Items.Cast<SoftwareStatisticalVM>().All(item => item.IsExpanded)
            : icHistory.Items.Count > 0 && icHistory.Items.Cast<HistoryEntryVM>().All(item => item.IsExpanded);

        private void UpdateExpandAllButton()
        {
            bool allExpanded = AllDetailsExpanded;
            expandAllIcon.Text = allExpanded ? "\uF497" : "\uF491";
            btnExpandAll.ToolTip = (allExpanded ? "折叠当前页的全部详情" : "展开当前页的全部详情").Translate();
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

        private void BtnSave_Click(object sender, RoutedEventArgs e) => RefreshData();

        private void Window_Closing(object sender, CancelEventArgs e) => plugin.winSetting = null;

        private void ShowBox_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
                plugin.Set.ShowDoing = ShowBox.IsChecked == true;
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
        private bool isExpanded;

        public SoftwareStatisticalVM(Setting.SoftwareStatistical stat, int todayMinute)
        {
            _stat = stat;
            TodayMinute = todayMinute;
        }

        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                if (isExpanded == value) return;
                isExpanded = value;
                OnPropertyChanged();
            }
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
        public string DurationText => _stat.Minute >= 60 ? $"{_stat.Minute / 60}h {_stat.Minute % 60}m" : $"{_stat.Minute}m";
        public DateTime LastUse => _stat.LastUse;
        public int TodayMinute { get; }
        public int TotalMinute => _stat.Minute;

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

    public class HistoryEntryVM : INotifyPropertyChanged
    {
        private bool isExpanded;
        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                if (isExpanded == value) return;
                isExpanded = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsExpanded)));
            }
        }

        public DateTime Date { get; set; }
        public string Header { get; set; } = string.Empty;
        public string SubHeader { get; set; } = string.Empty;
        public int Minute { get; set; }
        public string DurationText => Minute >= 60 ? $"{Minute / 60}h {Minute % 60}m" : $"{Minute}m";
        public List<Setting.DailyStatistical> Days { get; set; } = new();
        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public enum SortMode { LastUse, Name, TodayMinute, TotalMinute }
}
