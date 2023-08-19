using HKW.HKWViewModels.SimpleObservable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VPet.Plugin.ModMaker.Models;

public class I18nHelper
{
    public static I18nHelper Instance { get; set; } = new();
    public ObservableValue<string> CurrentLang { get; } = new();
    public ObservableCollection<string> Langs { get; } = new();

    public I18nHelper()
    {
        Langs.CollectionChanged += Langs_CollectionChanged;
    }

    private void Langs_CollectionChanged(
        object sender,
        System.Collections.Specialized.NotifyCollectionChangedEventArgs e
    )
    {
        // 替换
        if (e.NewStartingIndex == e.OldStartingIndex)
        {
            ReplaceLang?.Invoke((string)e.OldItems[0], (string)e.NewItems[0]);
            return;
        }
        // 删除
        if (e.OldItems is not null)
        {
            RemoveLang?.Invoke((string)e.OldItems[0]);
        }
        // 新增
        if (e.NewItems is not null)
        {
            AddLang?.Invoke((string)e.NewItems[0]);
        }
    }

    public event LangEventHandler AddLang;
    public event LangEventHandler RemoveLang;
    public event ReplaceLangEventHandler ReplaceLang;

    public delegate void LangEventHandler(string lang);
    public delegate void ReplaceLangEventHandler(string oldLang, string newLang);
}
