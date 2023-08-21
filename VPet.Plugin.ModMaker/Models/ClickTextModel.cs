using HKW.HKWViewModels.SimpleObservable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.ModMaker.Models;

public class ClickTextModel : II18nData<I18nClickTextModel>
{
    public ObservableValue<string> Id { get; } = new();
    public ObservableValue<ClickText.ModeType> Mode { get; } = new();

    public ObservableValue<string> Working { get; } = new();
    public ObservableValue<int> LikeMin { get; } = new();
    public ObservableValue<int> LikeMax { get; } = new();
    public ObservableValue<VPet_Simulator.Core.Main.WorkingState> WorkingState { get; } = new();
    public ObservableValue<ClickText.DayTime> DayTime { get; } = new();

    public ObservableValue<I18nClickTextModel> CurrentI18nData { get; } = new();
    public Dictionary<string, I18nClickTextModel> I18nDatas { get; } = new();

    public ClickTextModel()
    {
        foreach (var lang in I18nHelper.Current.CultureNames)
            I18nDatas.Add(lang, new());
        CurrentI18nData.Value = I18nDatas[I18nHelper.Current.CultureName.Value];
    }

    public ClickTextModel(ClickTextModel clickText)
        : this()
    {
        Mode.Value = clickText.Mode.Value;
        Working.Value = clickText.Working.Value;
        WorkingState.Value = clickText.WorkingState.Value;
        LikeMax.Value = clickText.LikeMax.Value;
        LikeMin.Value = clickText.LikeMin.Value;
        DayTime.Value = clickText.DayTime.Value;
        foreach (var item in clickText.I18nDatas)
            I18nDatas[item.Key] = item.Value;
        CurrentI18nData.Value = I18nDatas[I18nHelper.Current.CultureName.Value];
    }

    public ClickText ToClickText()
    {
        return new()
        {
            Mode = Mode.Value,
            Working = Working.Value,
            State = WorkingState.Value,
            LikeMax = LikeMax.Value,
            LikeMin = LikeMin.Value,
            DaiTime = DayTime.Value,
        };
    }
}

public class I18nClickTextModel
{
    public ObservableValue<string> Text { get; } = new();
}
