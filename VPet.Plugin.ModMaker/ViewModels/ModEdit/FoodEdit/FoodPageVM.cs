using HKW.HKWViewModels.SimpleObservable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VPet.Plugin.ModMaker.Models;
using VPet.Plugin.ModMaker.Views.ModEdit.FoodEdit;

namespace VPet.Plugin.ModMaker.ViewModels.ModEdit.FoodEdit;

public class FoodPageVM
{
    #region Value
    public ObservableValue<ObservableCollection<FoodModel>> ShowFoods { get; } = new();
    public ObservableCollection<FoodModel> Foods { get; } = new();
    public ObservableValue<string> FilterFoodText { get; } = new();
    #endregion
    #region Command
    public ObservableCommand AddFoodCommand { get; } = new();
    public ObservableCommand<FoodModel> EditFoodCommand { get; } = new();
    public ObservableCommand<FoodModel> RemoveFoodCommand { get; } = new();
    #endregion
    public FoodPageVM()
    {
        ShowFoods.Value = Foods;
        FilterFoodText.ValueChanged += FilterFoodText_ValueChanged;

        AddFoodCommand.ExecuteAction = AddFood;
        EditFoodCommand.ExecuteAction = EditFood;
        RemoveFoodCommand.ExecuteAction = RemoveFood;

        I18nHelper.Current.CultureName.ValueChanged += CurrentLang_ValueChanged;
        I18nHelper.Current.AddLang += Instance_AddLang;
        I18nHelper.Current.RemoveLang += Instance_RemoveLang;
        I18nHelper.Current.ReplaceLang += Instance_ReplaceLang;
    }

    private void CurrentLang_ValueChanged(string value)
    {
        foreach (var food in Foods)
        {
            food.CurrentI18nData.Value = food.I18nDatas[value];
        }
    }

    private void Instance_AddLang(string lang)
    {
        foreach (var food in Foods)
        {
            food.I18nDatas.Add(lang, new());
        }
    }

    private void Instance_RemoveLang(string lang)
    {
        foreach (var food in Foods)
        {
            food.I18nDatas.Remove(lang);
        }
    }

    private void Instance_ReplaceLang(string oldLang, string newLang)
    {
        foreach (var food in Foods)
        {
            var item = food.I18nDatas[oldLang];
            food.I18nDatas.Remove(oldLang);
            food.I18nDatas.Add(newLang, item);
        }
    }

    private void FilterFoodText_ValueChanged(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            ShowFoods.Value = Foods;
        }
        else
        {
            ShowFoods.Value = new(
                Foods.Where(f => f.CurrentI18nData.Value.Name.Value.Contains(value))
            );
        }
    }

    public void Close() { }

    private void AddFood()
    {
        var window = CreateAddFoodWindow();
        var vm = window.ViewModel;
        window.ShowDialog();
        if (window.IsCancel)
            return;
        Foods.Add(vm.Food.Value);
    }

    public void EditFood(FoodModel food)
    {
        var window = CreateAddFoodWindow();
        var vm = window.ViewModel;
        var newFood = vm.Food.Value = new(food);
        window.ShowDialog();
        if (window.IsCancel)
            return;
        if (ShowFoods.Value.Count == Foods.Count)
        {
            Foods[Foods.IndexOf(food)] = newFood;
        }
        else
        {
            Foods[Foods.IndexOf(food)] = newFood;
            ShowFoods.Value[ShowFoods.Value.IndexOf(food)] = newFood;
        }
        food.Close();
    }

    private void RemoveFood(FoodModel food)
    {
        if (MessageBox.Show("确定删除吗", "", MessageBoxButton.YesNo) is MessageBoxResult.No)
            return;
        if (ShowFoods.Value.Count == Foods.Count)
        {
            Foods.Remove(food);
        }
        else
        {
            ShowFoods.Value.Remove(food);
            Foods.Remove(food);
        }
    }

    private FoodEditWindow CreateAddFoodWindow()
    {
        var window = new FoodEditWindow();
        window.ViewModel.Foods = Foods;
        return window;
    }
}
