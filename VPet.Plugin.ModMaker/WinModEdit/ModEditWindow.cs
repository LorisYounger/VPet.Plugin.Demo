using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.ModMaker.WinModEdit;

public partial class ModEditWindow
{
    public static Food CreateFoodFormWindow(AddFoodWindow addFoodWindow)
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
            Strength = double.Parse(addFoodWindow.TextBox_Strength.Text),
            StrengthFood = double.Parse(addFoodWindow.TextBox_StrengthFood.Text),
            StrengthDrink = double.Parse(addFoodWindow.TextBox_StrengthDrink.Text),
            Health = double.Parse(addFoodWindow.TextBox_Health.Text),
            Feeling = double.Parse(addFoodWindow.TextBox_Feeling.Text),
            Likability = double.Parse(addFoodWindow.TextBox_Likability.Text),
            Price = double.Parse(addFoodWindow.TextBox_Price.Text),
            Exp = int.Parse(addFoodWindow.TextBox_Exp.Text),
        };
    }
}
