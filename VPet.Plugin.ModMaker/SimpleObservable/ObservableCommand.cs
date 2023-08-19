using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HKW.HKWViewModels.SimpleObservable;

/// <summary>
/// 可观察命令
/// </summary>
public class ObservableCommand : ICommand
{
    /// <summary>
    /// 执行的方法
    /// </summary>
    public Action? ExecuteAction { get; set; }

    /// <summary>
    /// 执行的异步方法
    /// </summary>
    public Func<Task>? ExecuteActionAsync { get; set; }

    /// <summary>
    /// 获取能否执行的方法
    /// </summary>
    public Func<bool>? CanExecuteAction { get; set; }

    /// <summary>
    /// 能执行的属性
    /// <para>
    /// 注意: 仅当 <see cref="CanExecuteAction"/> 为 <see langword="null"/> 时, 此属性才会被使用
    /// </para>
    /// </summary>
    public ObservableValue<bool> CanExecuteProperty { get; } = new(true);

    /// <summary>
    /// 等待异步执行完成
    /// </summary>
    private readonly ObservableValue<bool> r_waiting = new(false);

    /// <inheritdoc/>
    public ObservableCommand()
    {
        CanExecuteProperty.PropertyChanged += InvokeCanExecuteChanged;
        r_waiting.PropertyChanged += InvokeCanExecuteChanged;
    }

    private void InvokeCanExecuteChanged(
        object? sender,
        System.ComponentModel.PropertyChangedEventArgs e
    )
    {
        CanExecuteChanged?.Invoke(sender, e);
    }

    /// <summary>
    /// 能否被执行
    /// </summary>
    /// <param name="parameter">参数</param>
    /// <returns>能被执行为 <see langword="true"/> 否则为 <see langword="false"/></returns>
    public bool CanExecute(object? parameter)
    {
        if (r_waiting.Value is true)
            return false;
        return CanExecuteAction is null
            ? CanExecuteProperty.Value
            : CanExecuteAction?.Invoke() is not false;
    }

    /// <summary>
    /// 执行方法
    /// </summary>
    /// <param name="parameter">参数</param>
    public async void Execute(object? parameter)
    {
        ExecuteAction?.Invoke();
        await ExecuteAsync();
    }

    /// <summary>
    /// 执行异步方法, 会在等待中关闭按钮的可执行性, 完成后恢复
    /// </summary>
    /// <returns>等待</returns>
    private async Task ExecuteAsync()
    {
        if (ExecuteActionAsync is null)
            return;
        r_waiting.Value = true;
        await ExecuteActionAsync.Invoke();
        r_waiting.Value = false;
    }

    /// <summary>
    /// 能否执行属性被改变事件
    /// </summary>
    public event EventHandler? CanExecuteChanged;
}
