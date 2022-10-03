using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace MasterCsharpHosted.Shared.Services;

public class ModalService
{
    private readonly List<TaskCompletionSource<ModalResults>> _tasks = new();
    private readonly List<TaskCompletionSource<bool>> _confirmTasks = new();
    private readonly List<object> _modals = new();
    public bool IsOpen { get; set; }
    public ModalParameters Parameters { get; set; }
    public event Action<ModalResults> OnModalClose;
    public event Action OnOpen;
    public event Action<Type, ModalParameters, ModalOptions> OnOpenComponent;
    public event Action<RenderFragment<ModalService>, ModalParameters, ModalOptions> OnOpenFragment;
    public event Action<ModalConfirmOptions> OnConfirmOpen;
    public event Action<ModalParameters, ModalOptions> OnOpenSimple;
    public event Action<bool> OnModalConfirmClose;
    public void Open()
    {
        IsOpen = true;
        OnOpen.Invoke();
    }

    public Task<bool> ConfirmAsync(ModalConfirmOptions options)
    {
        TaskCompletionSource<bool> taskCompletionSource = new();
        _confirmTasks.Add(taskCompletionSource);
        _modals.Add(new object());
        OnConfirmOpen?.Invoke(options);
        return taskCompletionSource.Task;
    }
    public void Open<T>(ModalParameters parameters = null, ModalOptions options = null) where T : class
    {
        _modals.Add(new object());
        OnOpenComponent?.Invoke(typeof(T), parameters, options);
    }

    public void Open(RenderFragment<ModalService> renderFragment, ModalParameters parameters = null, ModalOptions options = null)
    {
        _modals.Add(new object());
        OnOpenFragment?.Invoke(renderFragment, parameters, options);
    }
    public Task<ModalResults> OpenAsync<T>(ModalParameters parameters = null, ModalOptions options = null) where T : class
    {
        TaskCompletionSource<ModalResults> taskCompletionSource = new();
        _tasks.Add(taskCompletionSource);
        Open<T>(parameters, options);
        return taskCompletionSource.Task;

    }
    //public Task Open<T>(ModalParameters parameters = null, ModalOptions options = null) where T : class
    //{
    //    OnOpenComponent?.Invoke(typeof(T), parameters, options);
    //    return Task.CompletedTask;
    //}
    public void Close(bool success)
    {
        var modal = _modals.LastOrDefault();
        if (modal == null) return;
        IsOpen = false;
        OnClose(ModalResults.Empty(success));
        _modals.Remove(modal);

    }
    public void CloseSelf(ModalResults results = null)
    {
        Close(results);
    }

    public void CloseConfirm(bool confirmed)
    {
        var modal = _modals.LastOrDefault();
        if (modal != null)
        {
            IsOpen = false;
            OnConfirmClose(confirmed);
            _modals.Remove(modal);
        }

        var taskCompletionSource = _confirmTasks.LastOrDefault();
        if (taskCompletionSource == null || taskCompletionSource.Task.IsCompleted) return;
        _confirmTasks.Remove(taskCompletionSource);
        taskCompletionSource.SetResult(confirmed);
    }
    public void Close(ModalResults result)
    {
        var modal = _modals.LastOrDefault();
        //ModalResults result = new(success, parameters);
        if (modal != null)
        {
            IsOpen = false;
            OnClose(result);
            _modals.Remove(modal);
        }
       
        var taskCompletionSource = _tasks.LastOrDefault();
        if (taskCompletionSource == null || taskCompletionSource.Task.IsCompleted) return;
        _tasks.Remove(taskCompletionSource);
        taskCompletionSource.SetResult(result);
    }
    private void OnClose(ModalResults results)
    {
        OnModalClose?.Invoke(results);
    }

    private void OnConfirmClose(bool confirmed)
    {
        OnModalConfirmClose?.Invoke(confirmed);
    }

}

public class ModalResults
{
    public ModalResults(bool success, ModalParameters parameters)
    {
        IsSuccess = success;
        Parameters = parameters;
    }
    public bool IsSuccess { get; private set; }
    public ModalParameters Parameters { get; private set; }

    public static ModalResults Empty(bool success)
    {
        return new ModalResults(success, new ModalParameters());
    }
}
public class ModalParameters : Dictionary<string, object>
{
    public T Get<T>(string parameterName)
    {
        if (!this.ContainsKey(parameterName))
            throw new KeyNotFoundException($"{parameterName} does not exist in modal parameters");

        return (T)this[parameterName];
    }
    public T Get<T>(string parameterName, T defaultValue)
    {
        if (TryGetValue(parameterName, out var parameterValue))
            return (T)parameterValue;
        parameterValue = defaultValue;
        this[parameterName] = parameterValue;
        return (T)parameterValue;
    }
    public void Set<T>(string parameterName, T parameterValue)
    {
        this[parameterName] = parameterValue;
    }
}

public class ModalButton
{
    public string Label { get; set; }
    public Action Action { get; set; }
}
public class ModalOptions
{
    public string Title { get; set; }
    public string Content { get; set; }
    public Location Location { get; set; } = Location.Center;
    public bool ShowCloseButton { get; set; }
    public string Height { get; set; } = "auto";
    public string Width { get; set; } = "auto";
    public bool CloseOnOuterClick { get; set; } = true;
}

public class ModalConfirmOptions
{
    public ModalConfirmOptions(string title)
    {
        Title = title;
    }
 
    public string Title { get; set; }
    public string Content { get; set; } = "Are you sure?";
    public ConfirmButton ConfirmButton { get; set; } = new("Yes");
    public DeclineButton DeclineButton { get; set; } = new("No");
}
public record ConfirmButton(string Label);
public record DeclineButton(string Label);