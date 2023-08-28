using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HKW.HKWViewModels.SimpleObservable;

/// <summary>
/// 带参数的可观察命令
/// </summary>
/// <typeparam name="T">参数类型</typeparam>
public class ObservableCommand<T> : ICommand
    where T : notnull
{
    /// <inheritdoc cref="ObservableCommand.ExecuteAction"/>
    public Action<T?>? ExecuteAction { get; set; }

    /// <inheritdoc cref="ObservableCommand.ExecuteActionAsync"/>
    public Func<T?, Task>? ExecuteActionAsync { get; set; }

    /// <inheritdoc cref="ObservableCommand.CanExecuteAction"/>
    public Func<T?, bool>? CanExecuteAction { get; set; }

    /// <inheritdoc cref="ObservableCommand.CanExecuteProperty"/>
    public ObservableValue<bool> CanExecuteProperty { get; } = new(true);

    /// <inheritdoc cref="ObservableCommand.r_waiting"/>
    private readonly ObservableValue<bool> r_waiting = new(false);

    /// <inheritdoc cref="ObservableCommand.ObservableCommand()"/>
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

    /// <inheritdoc cref="ObservableCommand.CanExecute(object?)"/>
    public bool CanExecute(object? parameter)
    {
        if (r_waiting.Value is true)
            return false;
        return CanExecuteAction is null
            ? CanExecuteProperty.Value
            : CanExecuteAction?.Invoke((T?)parameter) is not false;
    }

    /// <inheritdoc cref="ObservableCommand.Execute(object?)"/>
    public async void Execute(object? parameter)
    {
        ExecuteAction?.Invoke((T?)parameter);
        await ExecuteAsync((T?)parameter);
    }

    /// <inheritdoc cref="ObservableCommand.ExecuteActionAsync()"/>
    /// <param name="parameter">参数</param>
    private async Task ExecuteAsync(T? parameter)
    {
        if (ExecuteActionAsync is null)
            return;
        r_waiting.Value = true;
        await ExecuteActionAsync.Invoke(parameter);
        r_waiting.Value = false;
    }

    /// <inheritdoc cref="ObservableCommand.CanExecuteChanged"/>
    public event EventHandler? CanExecuteChanged;
}
