using HKW.HKWViewModels.SimpleObservable;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using VPet.Plugin.ModMaker.Models;
using System.Collections.Specialized;
using System.ComponentModel;
using VPet.Plugin.ModMaker.Views.ModEdit;
using System.Windows;

namespace VPet.Plugin.ModMaker.ViewModels.ModEdit;

public class ModEditWindowVM
{
    public ModEditWindow ModEditWindow { get; }

    #region Value
    public ObservableValue<BitmapImage> ModImage { get; } = new();
    public ObservableValue<ModInfoModel> ModInfo { get; } = new(new());
    public ObservableValue<string> CurrentLang { get; } = new();
    public I18nHelper I18nData => I18nHelper.Instance;
    #endregion

    #region Command
    public ObservableCommand AddImageCommand { get; } = new();
    public ObservableCommand ChangeImageCommand { get; } = new();
    public ObservableCommand AddLangCommand { get; } = new();

    public ObservableCommand<string> EditLangCommand { get; } = new();
    public ObservableCommand<string> RemoveLangCommand { get; } = new();
    #endregion

    public ModEditWindowVM() { }

    public ModEditWindowVM(ModEditWindow window)
    {
        ModEditWindow = window;

        I18nHelper.Instance.AddLang += I18nData_AddLang;
        I18nHelper.Instance.RemoveLang += I18nData_RemoveLang;
        I18nHelper.Instance.ReplaceLang += I18nData_ReplaceLang;
        CurrentLang.ValueChanged += CurrentLang_ValueChanged;

        AddImageCommand.ExecuteAction = AddImage;
        ChangeImageCommand.ExecuteAction = ChangeImage;
        AddLangCommand.ExecuteAction = AddLang;
        EditLangCommand.ExecuteAction = EditLang;
        RemoveLangCommand.ExecuteAction = RemoveLang;
    }

    private void I18nData_AddLang(string lang)
    {
        ModInfo.Value.I18nDatas.Add(lang, new());
    }

    private void I18nData_RemoveLang(string lang)
    {
        ModInfo.Value.I18nDatas.Remove(lang);
    }

    private void I18nData_ReplaceLang(string oldLang, string newLang)
    {
        var info = ModInfo.Value.I18nDatas[oldLang];
        ModInfo.Value.I18nDatas.Remove(oldLang);
        ModInfo.Value.I18nDatas.Add(newLang, info);
    }

    private void CurrentLang_ValueChanged(string value)
    {
        if (value is null)
            return;
        ModInfo.Value.CurrentI18nData.Value = ModInfo.Value.I18nDatas[value];
    }

    public void Close()
    {
        ModInfo.Value.ModImage.Value?.StreamSource?.Close();
    }

    private void AddImage()
    {
        OpenFileDialog openFileDialog =
            new() { Title = "选择图片", Filter = $"图片|*.jpg;*.jpeg;*.png;*.bmp" };
        if (openFileDialog.ShowDialog() is true)
        {
            ModInfo.Value.ModImage.Value = Utils.LoadImageToStream(openFileDialog.FileName);
        }
    }

    private void ChangeImage()
    {
        OpenFileDialog openFileDialog =
            new() { Title = "选择图片", Filter = $"图片|*.jpg;*.jpeg;*.png;*.bmp" };
        if (openFileDialog.ShowDialog() is true)
        {
            ModInfo.Value.ModImage.Value?.StreamSource?.Close();
            ModInfo.Value.ModImage.Value = Utils.LoadImageToStream(openFileDialog.FileName);
        }
    }

    private void AddLang()
    {
        var window = CreateAddLangWindow();
        window.ShowDialog();
        if (window.IsCancel)
            return;
        I18nHelper.Instance.Langs.Add(window.Lang.Value);
    }

    private void EditLang(string oldLang)
    {
        var window = CreateAddLangWindow();
        window.Lang.Value = oldLang;
        window.ShowDialog();
        if (window.IsCancel)
            return;
        I18nHelper.Instance.Langs[I18nHelper.Instance.Langs.IndexOf(oldLang)] = window.Lang.Value;
        CurrentLang.Value = window.Lang.Value;
    }

    private void RemoveLang(string oldLang)
    {
        if (MessageBox.Show("确定删除吗", "", MessageBoxButton.YesNo) is MessageBoxResult.No)
            return;
        I18nHelper.Instance.Langs.Remove(oldLang);
    }

    private Window_AddLang CreateAddLangWindow()
    {
        var window = new Window_AddLang();
        window.Langs = I18nHelper.Instance.Langs;
        return window;
    }
}
