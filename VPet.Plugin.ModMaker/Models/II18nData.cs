using HKW.HKWViewModels.SimpleObservable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VPet.Plugin.ModMaker.Models;

public interface II18nData<T>
    where T : class
{
    public ObservableValue<T> CurrentI18nData { get; }
    public Dictionary<string, T> I18nDatas { get; }
}
