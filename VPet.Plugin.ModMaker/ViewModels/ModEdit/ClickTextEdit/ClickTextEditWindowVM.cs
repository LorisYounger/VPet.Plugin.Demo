using HKW.HKWViewModels.SimpleObservable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VPet.Plugin.ModMaker.Models;
using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.ModMaker.ViewModels.ModEdit.ClickTextEdit;

public class ClickTextEditWindowVM
{
    public ObservableCollection<ClickTextModel> ClickTexts { get; set; }
    #region Value
    public ObservableValue<ClickTextModel> ClickText { get; } = new(new());
    public ObservableCollection<ClickText.ModeType> ModeTypes { get; } = new();
    public ObservableCollection<ClickText.DayTime> DayTimes { get; } = new();
    public ObservableCollection<VPet_Simulator.Core.Main.WorkingState> WorkingStates { get; } =
        new();
    #endregion
    public ClickTextEditWindowVM()
    {
        foreach (ClickText.ModeType item in Enum.GetValues(typeof(ClickText.ModeType)))
            ModeTypes.Add(item);
        foreach (ClickText.DayTime item in Enum.GetValues(typeof(ClickText.DayTime)))
            DayTimes.Add(item);
        foreach (
            VPet_Simulator.Core.Main.WorkingState item in Enum.GetValues(
                typeof(VPet_Simulator.Core.Main.WorkingState)
            )
        )
            WorkingStates.Add(item);
    }
}
