using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.ModMaker.ModEdit.FoodEdit;

public partial class Page_Food
{
    public static Food CreateFoodFromWindow(Window_AddFood addFoodWindow)
    {
        return new()
        {
            Name = addFoodWindow.TextBox_FoodName.Text,
            ImageSource = addFoodWindow.Image_FoodImage.Source,
            Desc = addFoodWindow.TextBox_FoodDescription.Text,
            Type = (Food.FoodType)
                Enum.Parse(
                    typeof(Food.FoodType),
                    ((ComboBoxItem)addFoodWindow.ComboBox_FoodType.SelectedItem).Tag.ToString()
                ),
            Strength = (double)addFoodWindow.NumberInput_Strength.Value,
            StrengthFood = (double)addFoodWindow.NumberInput_StrengthFood.Value,
            StrengthDrink = (double)addFoodWindow.NumberInput_StrengthDrink.Value,
            Health = (double)addFoodWindow.NumberInput_Health.Value,
            Feeling = (double)addFoodWindow.NumberInput_Feeling.Value,
            Likability = (double)addFoodWindow.NumberInput_Likability.Value,
            Price = (double)addFoodWindow.NumberInput_Price.Value,
            Exp = (int)addFoodWindow.NumberInput_Exp.Value,
        };
    }

    public void ChangeFoodInfo(Food oldFood)
    {
        var window = new Window_AddFood();
        window.TextBox_FoodName.Text = oldFood.Name;
        window.Image_FoodImage.Source = oldFood.ImageSource;
        window.TextBox_FoodDescription.Text = oldFood.Desc;
        foreach (ComboBoxItem item in window.ComboBox_FoodType.Items)
            if (item.Tag.ToString() == oldFood.Type.ToString())
                window.ComboBox_FoodType.SelectedItem = item;
        window.NumberInput_Strength.Value = oldFood.Strength;
        window.NumberInput_StrengthFood.Value = oldFood.StrengthFood;
        window.NumberInput_StrengthDrink.Value = oldFood.StrengthDrink;
        window.NumberInput_Health.Value = oldFood.Health;
        window.NumberInput_Feeling.Value = oldFood.Feeling;
        window.NumberInput_Likability.Value = oldFood.Likability;
        window.NumberInput_Price.Value = oldFood.Price;
        window.NumberInput_Exp.Value = oldFood.Exp;
        window.Button_AddFoodImage.Visibility = Visibility.Visible;
        window.Closed += (s, e) =>
        {
            if (s is not Window_AddFood addFoodWindow || addFoodWindow.IsCancel)
                return;
            var food = CreateFoodFromWindow(addFoodWindow);
            if (FoodDict.TryGetValue(food.Name, out var tempFood))
            {
                FoodDict[food.Name] = Foods[Foods.IndexOf(tempFood)] = food;
            }
            else
            {
                Foods[Foods.IndexOf(oldFood)] = food;
                FoodDict.Remove(oldFood.Name);
                FoodDict.Add(food.Name, food);
            }
        };
        ShowDialogX(window);
    }

    public event ShowDialogXHandler ShowDialogX;
}
