using HKW.HKWViewModels.SimpleObservable;
using System;
using System.Collections.Generic;
using System.IO;
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
        foreach (var lang in I18nHelper.Current.CultureNames)
            I18nDatas.Add(lang, new());
        CurrentI18nData.Value = I18nDatas[I18nHelper.Current.CultureName.Value];
    }

    public FoodModel(FoodModel food)
        : this()
    {
        Id.Value = food.Id.Value;
        Type.Value = food.Type.Value;
        Strength.Value = food.Strength.Value;
        StrengthFood.Value = food.StrengthFood.Value;
        StrengthDrink.Value = food.StrengthDrink.Value;
        Feeling.Value = food.Feeling.Value;
        Health.Value = food.Health.Value;
        Likability.Value = food.Likability.Value;
        Price.Value = food.Price.Value;
        Exp.Value = food.Exp.Value;
        Image.Value = Utils.LoadImageToStream(food.Image.Value);
        foreach (var item in food.I18nDatas)
            I18nDatas[item.Key] = item.Value;
        CurrentI18nData.Value = I18nDatas[I18nHelper.Current.CultureName.Value];
    }

    public Food ToFood()
    {
        // 没有 Name 和 Description
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
        };
    }

    public void Close()
    {
        Image.Value?.StreamSource?.Close();
    }
}

public class I18nFoodModel
{
    public ObservableValue<string> Name { get; } = new();
    public ObservableValue<string> Description { get; } = new();
}
