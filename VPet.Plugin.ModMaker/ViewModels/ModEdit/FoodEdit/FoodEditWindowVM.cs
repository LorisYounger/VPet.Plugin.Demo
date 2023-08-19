using HKW.HKWViewModels.SimpleObservable;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using VPet.Plugin.ModMaker.Models;
using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.ModMaker.ViewModels.ModEdit.FoodEdit;

public class FoodEditWindowVM
{
    public ObservableCollection<FoodModel> Foods { get; set; }

    #region Value
    public ObservableValue<FoodModel> Food { get; } = new(new());
    public ObservableCollection<Food.FoodType> FoodTypes { get; } = new();
    #endregion

    #region Command
    public ObservableCommand AddImageCommand { get; } = new();
    public ObservableCommand ChangeImageCommand { get; } = new();
    #endregion

    public FoodEditWindowVM()
    {
        InitializeFoodTypes();
        AddImageCommand.ExecuteAction = AddImage;
        ChangeImageCommand.ExecuteAction = ChangeImage;
    }

    public void Close() { }

    private void InitializeFoodTypes()
    {
        foreach (Food.FoodType foodType in Enum.GetValues(typeof(Food.FoodType)))
        {
            FoodTypes.Add(foodType);
        }
    }

    private void AddImage()
    {
        OpenFileDialog openFileDialog =
            new() { Title = "选择图片", Filter = $"图片|*.jpg;*.jpeg;*.png;*.bmp" };
        if (openFileDialog.ShowDialog() is true)
        {
            Food.Value.Image.Value = Utils.LoadImageToStream(openFileDialog.FileName);
        }
    }

    private void ChangeImage()
    {
        OpenFileDialog openFileDialog =
            new() { Title = "选择图片", Filter = $"图片|*.jpg;*.jpeg;*.png;*.bmp" };
        if (openFileDialog.ShowDialog() is true)
        {
            Food.Value.Image.Value?.StreamSource?.Close();
            Food.Value.Image.Value = Utils.LoadImageToStream(openFileDialog.FileName);
        }
    }
}
