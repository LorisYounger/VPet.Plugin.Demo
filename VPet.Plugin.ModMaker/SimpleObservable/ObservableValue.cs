using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HKW.HKWViewModels.SimpleObservable;

/// <summary>
/// 可观察值
/// </summary>
/// <typeparam name="T"></typeparam>
public class ObservableValue<T> : INotifyPropertyChanging, INotifyPropertyChanged
    where T : notnull
{
    private T? _value = default;

    /// <summary>
    /// 当前值
    /// </summary>
    public T? Value
    {
        get => _value;
        set
        {
            if (_value?.Equals(value) is true)
                return;
            PropertyChanging?.Invoke(this, new(nameof(Value)));
            ValueChanging?.Invoke(_value, value);
            _value = value;
            PropertyChanged?.Invoke(this, new(nameof(Value)));
            ValueChanged?.Invoke(value);
        }
    }

    /// <inheritdoc/>
    public ObservableValue() { }

    /// <inheritdoc/>
    /// <param name="value">值</param>
    public ObservableValue(T value)
    {
        _value = value;
    }

    /// <summary>
    /// 属性改变前事件
    /// </summary>
    public event PropertyChangingEventHandler? PropertyChanging;

    /// <summary>
    /// 属性改变后事件
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// 值改变前事件
    /// </summary>
    public event ValueChangingEventHandler? ValueChanging;

    /// <summary>
    /// 值改变后事件
    /// </summary>
    public event ValueChangedEventHandler? ValueChanged;

    /// <summary>
    /// 值改变后事件方法
    /// </summary>
    /// <param name="value">值</param>
    public delegate void ValueChangedEventHandler(T? value);

    /// <summary>
    /// 值改变前事件方法
    /// </summary>
    /// <param name="oldValue">旧值</param>
    /// <param name="newValue">新值</param>
    public delegate void ValueChangingEventHandler(T? oldValue, T? newValue);
}
