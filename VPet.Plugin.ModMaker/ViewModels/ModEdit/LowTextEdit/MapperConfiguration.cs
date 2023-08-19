using System;

namespace VPet.Plugin.ModMaker.ViewModels.ModEdit.LowTextEdit;
internal class MapperConfiguration
{
    private Func<object, object> value;

    public MapperConfiguration(Func<object, object> value)
    {
        this.value = value;
    }
}