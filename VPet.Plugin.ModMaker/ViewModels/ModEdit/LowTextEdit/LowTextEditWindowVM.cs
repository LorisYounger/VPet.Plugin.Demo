using HKW.HKWViewModels.SimpleObservable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VPet.Plugin.ModMaker.Models;
using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.ModMaker.ViewModels.ModEdit.LowTextEdit;

public class LowTextEditWindowVM
{
    #region Value
    public ObservableCollection<LowTextModel> LowTexts { get; set; }
    public ObservableValue<LowTextModel> LowText { get; } = new(new());

    public ObservableCollection<LowText.ModeType> LowTextModeTypes { get; } = new();
    public ObservableCollection<LowText.LikeType> LowTextLikeTypes { get; } = new();
    public ObservableCollection<LowText.StrengthType> LowTextStrengthTypes { get; } = new();
    #endregion

    public LowTextEditWindowVM()
    {
        foreach (LowText.ModeType mode in Enum.GetValues(typeof(LowText.ModeType)))
            LowTextModeTypes.Add(mode);
        foreach (LowText.LikeType mode in Enum.GetValues(typeof(LowText.LikeType)))
            LowTextLikeTypes.Add(mode);
        foreach (LowText.StrengthType mode in Enum.GetValues(typeof(LowText.StrengthType)))
            LowTextStrengthTypes.Add(mode);
    }
}
