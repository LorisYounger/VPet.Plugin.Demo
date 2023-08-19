using HKW.HKWViewModels.SimpleObservable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.ModMaker.Models;

public class FoodModel : II18nData<I18nFoodModel>
{
    public ObservableValue<string> Id { get; } = new();
    public ObservableValue<Food.FoodType> Type { get; } = new();
    public ObservableValue<double> Strength { get; } = new();
    public ObservableValue<double> StrengthFood { get; } = new();
    public ObservableValue<double> StrengthDrink { get; } = new();
    public ObservableValue<double> Feeling { get; } = new();
    public ObservableValue<double> Health { get; } = new();
    public ObservableValue<double> Likability { get; } = new();
    public ObservableValue<double> Price { get; } = new();
    public ObservableValue<int> Exp { get; } = new();
    public ObservableValue<BitmapImage> Image { get; } = new();
    public ObservableValue<I18nFoodModel> CurrentI18nData { get; } = new();
    public Dictionary<string, I18nFoodModel> I18nDatas { get; } = new();

    public FoodModel()
    {
        foreach (var lang in I18nHelper.Instance.Langs)
            I18nDatas.Add(lang, new());
        CurrentI18nData.Value = I18nDatas[I18nHelper.Instance.CurrentLang.Value];
    }

    public Food ToFood()
    {
        return new Food()
        {
            Type = Type.Value,
            Strength = Strength.Value,
            StrengthFood = StrengthFood.Value,
            StrengthDrink = StrengthDrink.Value,
            Feeling = Feeling.Value,
            Health = Health.Value,
            Likability = Likability.Value,
            Price = Price.Value,
            ImageSource = Image.Value,
            Name = CurrentI18nData is null
                ? I18nDatas.First().Value.Name.Value
                : CurrentI18nData.Value.Name.Value,
            Desc = CurrentI18nData is null
                ? I18nDatas.First().Value.Description.Value
                : CurrentI18nData.Value.Description.Value,
        };
    }
}

public class I18nFoodModel
{
    public ObservableValue<string> Name { get; } = new();
    public ObservableValue<string> Description { get; } = new();
}
