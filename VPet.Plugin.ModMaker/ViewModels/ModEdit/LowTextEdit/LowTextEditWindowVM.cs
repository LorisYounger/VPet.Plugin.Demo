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
    public ObservableCollection<LowTextModel> LowTexts { get; set; }

    #region Value
    public ObservableValue<LowTextModel> LowText { get; } = new(new());

    public ObservableCollection<LowText.ModeType> ModeTypes { get; } = new();
    public ObservableCollection<LowText.LikeType> LikeTypes { get; } = new();
    public ObservableCollection<LowText.StrengthType> StrengthTypes { get; } = new();
    #endregion

    public LowTextEditWindowVM()
    {
        foreach (LowText.ModeType mode in Enum.GetValues(typeof(LowText.ModeType)))
            ModeTypes.Add(mode);
        foreach (LowText.LikeType mode in Enum.GetValues(typeof(LowText.LikeType)))
            LikeTypes.Add(mode);
        foreach (LowText.StrengthType mode in Enum.GetValues(typeof(LowText.StrengthType)))
            StrengthTypes.Add(mode);
    }
}
