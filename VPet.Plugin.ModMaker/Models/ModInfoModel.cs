using HKW.HKWViewModels.SimpleObservable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.ModMaker.Models;

public class ModInfoModel
{
    public ObservableValue<string> Author { get; } = new();
    public ObservableValue<string> GameVersion { get; } = new();
    public ObservableValue<string> ModVersion { get; } = new();
    public ObservableValue<BitmapImage> ModImage { get; } = new();
    public ObservableValue<I18nModInfoModel> CurrentI18nData { get; } = new();
    public Dictionary<string, I18nModInfoModel> I18nDatas { get; } = new();
    public List<FoodModel> Foods { get; set; } = new();
    //public List<ClickText> ClickTexts { get; set; } = new();
    //public List<LowText> LowTexts { get; set; } = new();
}

public class I18nModInfoModel
{
    public ObservableValue<string> Name { get; set; } = new();
    public ObservableValue<string> Description { get; set; } = new();
}
