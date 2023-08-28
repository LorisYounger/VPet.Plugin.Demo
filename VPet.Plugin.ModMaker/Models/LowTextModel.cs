using HKW.HKWViewModels.SimpleObservable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.ModMaker.Models;

public class LowTextModel : II18nData<I18nLowTextModel>
{
    public ObservableValue<string> Id { get; } = new();
    public ObservableValue<LowText.ModeType> Mode { get; } = new();
    public ObservableValue<LowText.StrengthType> Strength { get; } = new();
    public ObservableValue<LowText.LikeType> Like { get; } = new();

    public ObservableValue<I18nLowTextModel> CurrentI18nData { get; } = new();
    public Dictionary<string, I18nLowTextModel> I18nDatas { get; } = new();

    public LowTextModel()
    {
        foreach (var lang in I18nHelper.Current.CultureNames)
            I18nDatas.Add(lang, new());
        CurrentI18nData.Value = I18nDatas[I18nHelper.Current.CultureName.Value];
    }

    public LowTextModel(LowTextModel lowText)
        : this()
    {
        Mode = lowText.Mode;
        Strength = lowText.Strength;
        Like = lowText.Like;
        foreach (var item in lowText.I18nDatas)
            I18nDatas[item.Key] = item.Value;
        CurrentI18nData.Value = I18nDatas[I18nHelper.Current.CultureName.Value];
    }

    public void Close() { }

    public LowText ToLowText()
    {
        // 没有 Text
        return new()
        {
            Mode = Mode.Value,
            Strength = Strength.Value,
            Like = Like.Value,
        };
    }
}

public class I18nLowTextModel
{
    public ObservableValue<string> Text { get; } = new();
}
