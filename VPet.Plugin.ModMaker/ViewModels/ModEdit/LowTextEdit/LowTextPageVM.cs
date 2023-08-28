using HKW.HKWViewModels.SimpleObservable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VPet.Plugin.ModMaker.Models;
using VPet.Plugin.ModMaker.Views.ModEdit.LowTextEdit;
using Expression = System.Linq.Expressions.Expression;

namespace VPet.Plugin.ModMaker.ViewModels.ModEdit.LowTextEdit;

public class LowTextPageVM
{
    #region Value
    public ObservableValue<string> FilterLowText { get; } = new();
    public ObservableValue<ObservableCollection<LowTextModel>> ShowLowTexts { get; } = new();
    public ObservableCollection<LowTextModel> LowTexts { get; } = new();
    #endregion
    #region Command
    public ObservableCommand AddLowTextCommand { get; } = new();
    public ObservableCommand<LowTextModel> EditLowTextCommand { get; } = new();
    public ObservableCommand<LowTextModel> RemoveLowTextCommand { get; } = new();
    #endregion

    public LowTextPageVM()
    {
        ShowLowTexts.Value = LowTexts;
        FilterLowText.ValueChanged += FilterLowText_ValueChanged;
        AddLowTextCommand.ExecuteAction = AddLowText;
        EditLowTextCommand.ExecuteAction = EditLowText;
        RemoveLowTextCommand.ExecuteAction = RemoveLowText;

        I18nHelper.Current.CultureName.ValueChanged += CurrentLang_ValueChanged;
        I18nHelper.Current.AddLang += Instance_AddLang;
        I18nHelper.Current.RemoveLang += Instance_RemoveLang;
        I18nHelper.Current.ReplaceLang += Instance_ReplaceLang;
    }

    private void FilterLowText_ValueChanged(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            ShowLowTexts.Value = LowTexts;
        }
        else
        {
            ShowLowTexts.Value = new(
                LowTexts.Where(f => f.CurrentI18nData.Value.Text.Value.Contains(value))
            );
        }
    }

    private void AddLowText()
    {
        var window = CreateLowTextEditWindow();
        var vm = window.ViewModel;
        window.ShowDialog();
        if (window.IsCancel)
            return;
        LowTexts.Add(vm.LowText.Value);
    }

    public void EditLowText(LowTextModel lowText)
    {
        var window = CreateLowTextEditWindow();
        var vm = window.ViewModel;
        var newLowTest = vm.LowText.Value = new(lowText);
        window.ShowDialog();
        if (window.IsCancel)
            return;
        if (ShowLowTexts.Value.Count == LowTexts.Count)
        {
            LowTexts[LowTexts.IndexOf(lowText)] = newLowTest;
        }
        else
        {
            LowTexts[LowTexts.IndexOf(lowText)] = newLowTest;
            ShowLowTexts.Value[ShowLowTexts.Value.IndexOf(lowText)] = newLowTest;
        }
    }

    private void RemoveLowText(LowTextModel lowText)
    {
        if (MessageBox.Show("确定删除吗", "", MessageBoxButton.YesNo) is MessageBoxResult.No)
            return;
        if (ShowLowTexts.Value.Count == LowTexts.Count)
        {
            LowTexts.Remove(lowText);
        }
        else
        {
            ShowLowTexts.Value.Remove(lowText);
            LowTexts.Remove(lowText);
        }
    }

    private void CurrentLang_ValueChanged(string value)
    {
        foreach (var lowText in LowTexts)
        {
            lowText.CurrentI18nData.Value = lowText.I18nDatas[value];
        }
    }

    private void Instance_AddLang(string lang)
    {
        foreach (var lowText in LowTexts)
        {
            lowText.I18nDatas.Add(lang, new());
        }
    }

    private void Instance_RemoveLang(string lang)
    {
        foreach (var lowText in LowTexts)
        {
            lowText.I18nDatas.Remove(lang);
        }
    }

    private void Instance_ReplaceLang(string oldLang, string newLang)
    {
        foreach (var lowText in LowTexts)
        {
            var item = lowText.I18nDatas[oldLang];
            lowText.I18nDatas.Remove(oldLang);
            lowText.I18nDatas.Add(newLang, item);
        }
    }

    private LowTextEditWindow CreateLowTextEditWindow()
    {
        var window = new LowTextEditWindow();
        window.ViewModel.LowTexts = LowTexts;
        return window;
    }
}
