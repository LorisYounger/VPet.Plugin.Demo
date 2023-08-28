using HKW.HKWViewModels.SimpleObservable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VPet.Plugin.ModMaker.Models;

namespace VPet.Plugin.ModMaker.ViewModels.ModEdit.PetEdit;

public class PetEditWindowVM
{
    public ObservableValue<PetModel> Pet { get; } = new(new());

    public PetEditWindowVM() { }
}
