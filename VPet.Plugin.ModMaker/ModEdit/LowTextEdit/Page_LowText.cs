using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.ModMaker.ModEdit.LowTextEdit;

public partial class Page_LowText
{
    public static LowText CreateLowTextFromWindow(Window_AddLowText window)
    {
        var lowText = new LowText();
        lowText.Text = window.TextBox_Text.Text;
        lowText.Mode = (LowText.ModeType)
            Enum.Parse(
                typeof(LowText.ModeType),
                ((ComboBoxItem)window.ComboBox_ModeType.SelectedItem).Tag.ToString()
            );
        lowText.Strength = (LowText.StrengthType)
            Enum.Parse(
                typeof(LowText.StrengthType),
                ((ComboBoxItem)window.ComboBox_StrengthType.SelectedItem).Tag.ToString()
            );
        lowText.Like = (LowText.LikeType)
            Enum.Parse(
                typeof(LowText.LikeType),
                ((ComboBoxItem)window.ComboBox_LikeType.SelectedItem).Tag.ToString()
            );
        return lowText;
    }

    public void ChangeLowText(LowText oldText)
    {
        var window = new Window_AddLowText();
        window.TextBox_Text.Text = oldText.Text;
        foreach (ComboBoxItem item in window.ComboBox_ModeType.Items)
            if (item.Tag.ToString() == oldText.Mode.ToString())
                window.ComboBox_ModeType.SelectedItem = item;
        foreach (ComboBoxItem item in window.ComboBox_StrengthType.Items)
            if (item.Tag.ToString() == oldText.Strength.ToString())
                window.ComboBox_StrengthType.SelectedItem = item;
        foreach (ComboBoxItem item in window.ComboBox_LikeType.Items)
            if (item.Tag.ToString() == oldText.Like.ToString())
                window.ComboBox_LikeType.SelectedItem = item;
        window.Closed += (s, e) =>
        {
            if (s is not Window_AddLowText lowTextWindow || lowTextWindow.IsCancel)
                return;
            var food = CreateLowTextFromWindow(lowTextWindow);
            if (LowTextDict.TryGetValue(food.Text, out var tempLowText))
            {
                LowTextDict[food.Text] = LowTexts[LowTexts.IndexOf(tempLowText)] = food;
            }
            else
            {
                LowTexts[LowTexts.IndexOf(oldText)] = food;
                LowTextDict.Remove(oldText.Text);
                LowTextDict.Add(food.Text, food);
            }
        };
        ShowDialogX(window);
    }

    public event ShowDialogXHandler ShowDialogX;
}
