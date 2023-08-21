using HKW.HKWViewModels.SimpleObservable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VPet.Plugin.ModMaker.Models;
using VPet.Plugin.ModMaker.Views;
using VPet.Plugin.ModMaker.Views.ModEdit;

namespace VPet.Plugin.ModMaker.ViewModels;

public class WindowVM_ModMaker
{
    public ModMakerWindow ModMakerWindow { get; }

    public ModEditWindow ModEditWindow { get; private set; }

    public ObservableCommand CreateNewModCommand { get; set; } =
        new() { ExecuteAction = () => { } };

    public WindowVM_ModMaker() { }

    public WindowVM_ModMaker(ModMakerWindow window)
    {
        ModMakerWindow = window;
        CreateNewModCommand.ExecuteAction = CreateNewMod;
    }

    private void CreateNewMod()
    {
        I18nHelper.Current = new();
        ModEditWindow = new();
        ModEditWindow.Show();
        ModMakerWindow.Hide();
        ModEditWindow.Closed += (s, e) =>
        {
            ModMakerWindow.Close();
        };
    }
}
