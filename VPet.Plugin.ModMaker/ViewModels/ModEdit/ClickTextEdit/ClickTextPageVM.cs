using HKW.HKWViewModels.SimpleObservable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VPet.Plugin.ModMaker.Models;
using VPet.Plugin.ModMaker.Views.ModEdit.ClickTextEdit;

namespace VPet.Plugin.ModMaker.ViewModels.ModEdit.ClickTextEdit;

public class ClickTextPageVM
{
    #region Value
    public ObservableValue<ObservableCollection<ClickTextModel>> ShowClickTexts { get; } = new();
    public ObservableCollection<ClickTextModel> ClickTexts { get; } = new();
    public ObservableValue<string> FilterClickText { get; } = new();
    #endregion
    #region Command
    public ObservableCommand AddClickTextCommand { get; } = new();
    public ObservableCommand<ClickTextModel> EditClickTextCommand { get; } = new();
    public ObservableCommand<ClickTextModel> RemoveClickTextCommand { get; } = new();
    #endregion

    public ClickTextPageVM()
    {
        ShowClickTexts.Value = ClickTexts;
        FilterClickText.ValueChanged += FilterClickText_ValueChanged;
        AddClickTextCommand.ExecuteAction = AddClickText;
        EditClickTextCommand.ExecuteAction = EditClickText;
        RemoveClickTextCommand.ExecuteAction = RemoveClickText;

        I18nHelper.Current.CultureName.ValueChanged += CurrentLang_ValueChanged;
        I18nHelper.Current.AddLang += Instance_AddLang;
        I18nHelper.Current.RemoveLang += Instance_RemoveLang;
        I18nHelper.Current.ReplaceLang += Instance_ReplaceLang;
    }

    private void FilterClickText_ValueChanged(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            ShowClickTexts.Value = ClickTexts;
        }
        else
        {
            ShowClickTexts.Value = new(
                ClickTexts.Where(f => f.CurrentI18nData.Value.Text.Value.Contains(value))
            );
        }
    }

    private void AddClickText()
    {
        var window = CreateClickTextEditWindow();
        var vm = window.ViewModel;
        window.ShowDialog();
        if (window.IsCancel)
            return;
        ClickTexts.Add(vm.ClickText.Value);
    }

    public void EditClickText(ClickTextModel clickText)
    {
        var window = CreateClickTextEditWindow();
        var vm = window.ViewModel;
        var newLowTest = vm.ClickText.Value = new(clickText);
        window.ShowDialog();
        if (window.IsCancel)
            return;
        if (ShowClickTexts.Value.Count == ClickTexts.Count)
        {
            ClickTexts[ClickTexts.IndexOf(clickText)] = newLowTest;
        }
        else
        {
            ClickTexts[ClickTexts.IndexOf(clickText)] = newLowTest;
            ShowClickTexts.Value[ShowClickTexts.Value.IndexOf(clickText)] = newLowTest;
        }
    }

    private void RemoveClickText(ClickTextModel clickText)
    {
        if (MessageBox.Show("确定删除吗", "", MessageBoxButton.YesNo) is MessageBoxResult.No)
            return;
        if (ShowClickTexts.Value.Count == ClickTexts.Count)
        {
            ClickTexts.Remove(clickText);
        }
        else
        {
            ShowClickTexts.Value.Remove(clickText);
            ClickTexts.Remove(clickText);
        }
    }

    private void CurrentLang_ValueChanged(string value)
    {
        foreach (var lowText in ClickTexts)
        {
            lowText.CurrentI18nData.Value = lowText.I18nDatas[value];
        }
    }

    private void Instance_AddLang(string lang)
    {
        foreach (var lowText in ClickTexts)
        {
            lowText.I18nDatas.Add(lang, new());
        }
    }

    private void Instance_RemoveLang(string lang)
    {
        foreach (var lowText in ClickTexts)
        {
            lowText.I18nDatas.Remove(lang);
        }
    }

    private void Instance_ReplaceLang(string oldLang, string newLang)
    {
        foreach (var lowText in ClickTexts)
        {
            var item = lowText.I18nDatas[oldLang];
            lowText.I18nDatas.Remove(oldLang);
            lowText.I18nDatas.Add(newLang, item);
        }
    }

    private ClickTextEditWindow CreateClickTextEditWindow()
    {
        var window = new ClickTextEditWindow();
        window.ViewModel.ClickTexts = ClickTexts;
        return window;
    }
}
