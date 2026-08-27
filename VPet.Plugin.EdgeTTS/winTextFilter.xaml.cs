using LinePutScript.Localization.WPF;
using System;
using System.Collections.Generic;
using System.Windows;

namespace VPet.Plugin.VPetTTS
{
    /// <summary>
    /// winTextFilter.xaml 的交互逻辑
    /// 选择哪些括号里的动作描写不参与朗读，并给出实时预览
    /// </summary>
    public partial class winTextFilter : Window
    {
        private const string SampleText = "（歪着头看你）今天也辛苦了呀，<动作>轻轻摸了摸主人的头</动作>要不要休息一下？";

        private readonly TextFilterSetting setting;
        private bool loaded;

        /// <summary>
        /// 用户点了确定时为 true，勾选结果已写回传入的实例
        /// </summary>
        public bool Confirmed { get; private set; }

        public winTextFilter(TextFilterSetting setting)
        {
            InitializeComponent();
            Resources = Application.Current.Resources;
            this.setting = setting ?? new TextFilterSetting();

            FilterRound.IsChecked = this.setting.RoundBracket;
            FilterSquare.IsChecked = this.setting.SquareBracket;
            FilterCurly.IsChecked = this.setting.CurlyBracket;
            FilterAngle.IsChecked = this.setting.AngleBracket;
            FilterBookTitle.IsChecked = this.setting.BookTitleMark;
            FilterPairedTag.IsChecked = this.setting.PairedTag;
            FilterAsterisk.IsChecked = this.setting.Asterisk;
            FilterCustomPairs.Text = this.setting.CustomPairs ?? "";
            FilterCustomRegex.Text = this.setting.CustomRegex ?? "";
            PreviewInput.Text = SampleText;

            loaded = true;

            FilterRound.Checked += OnOptionChanged;
            FilterRound.Unchecked += OnOptionChanged;
            FilterSquare.Checked += OnOptionChanged;
            FilterSquare.Unchecked += OnOptionChanged;
            FilterCurly.Checked += OnOptionChanged;
            FilterCurly.Unchecked += OnOptionChanged;
            FilterAngle.Checked += OnOptionChanged;
            FilterAngle.Unchecked += OnOptionChanged;
            FilterBookTitle.Checked += OnOptionChanged;
            FilterBookTitle.Unchecked += OnOptionChanged;
            FilterPairedTag.Checked += OnOptionChanged;
            FilterPairedTag.Unchecked += OnOptionChanged;
            FilterAsterisk.Checked += OnOptionChanged;
            FilterAsterisk.Unchecked += OnOptionChanged;
            //TextChanged 用的是 TextChangedEventHandler，签名和 RoutedEventHandler 不通用
            FilterCustomPairs.TextChanged += (s, e) => UpdatePreview();
            FilterCustomRegex.TextChanged += (s, e) => UpdatePreview();
            PreviewInput.TextChanged += (s, e) => UpdatePreview();

            UpdatePreview();
        }

        private void OnOptionChanged(object sender, RoutedEventArgs e) => UpdatePreview();

        /// <summary>
        /// 把当前勾选状态收集成一份临时设置，用于预览和写回
        /// </summary>
        private TextFilterSetting CollectSetting() => new TextFilterSetting
        {
            Enable = true,
            RoundBracket = FilterRound.IsChecked == true,
            SquareBracket = FilterSquare.IsChecked == true,
            CurlyBracket = FilterCurly.IsChecked == true,
            AngleBracket = FilterAngle.IsChecked == true,
            BookTitleMark = FilterBookTitle.IsChecked == true,
            PairedTag = FilterPairedTag.IsChecked == true,
            Asterisk = FilterAsterisk.IsChecked == true,
            CustomPairs = FilterCustomPairs.Text ?? "",
            CustomRegex = FilterCustomRegex.Text ?? ""
        };

        private void UpdatePreview()
        {
            if (!loaded)
                return;

            try
            {
                var errors = new List<string>();
                var filtered = SpeechTextFilter.Apply(PreviewInput.Text, CollectSetting(), errors);
                PreviewOutput.Text = string.IsNullOrWhiteSpace(filtered)
                    ? "（整句都是动作描写，本次不会发声）".Translate()
                    : filtered;

                //写错的正则在这里当场提示，免得保存完才发现规则没生效
                if (errors.Count > 0)
                {
                    RegexError.Text = string.Join(Environment.NewLine, errors);
                    RegexError.Visibility = Visibility.Visible;
                }
                else
                {
                    RegexError.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                PreviewOutput.Text = "预览失败: {0}".Translate(ex.Message);
            }
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            var collected = CollectSetting();
            setting.RoundBracket = collected.RoundBracket;
            setting.SquareBracket = collected.SquareBracket;
            setting.CurlyBracket = collected.CurlyBracket;
            setting.AngleBracket = collected.AngleBracket;
            setting.BookTitleMark = collected.BookTitleMark;
            setting.PairedTag = collected.PairedTag;
            setting.Asterisk = collected.Asterisk;
            setting.CustomPairs = collected.CustomPairs;
            setting.CustomRegex = collected.CustomRegex;

            Confirmed = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => Close();
    }
}
