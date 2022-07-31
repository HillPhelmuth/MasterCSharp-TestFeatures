using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterCsharpHosted.Shared.Services;

public class ModalService
{
    private List<TaskCompletionSource<ModalResults>> tasks = new();
    private List<object> modals = new();
    public bool IsOpen { get; set; }
    public ModalParameters Parameters { get; set; }
    public event Action<ModalResults> OnModalClose;
    public event Action OnOpen;
    public event Action<Type, ModalParameters, ModalOptions> OnOpenComponent;
    public event Action<ModalParameters, ModalOptions> OnOpenSimple;
    public void Open()
    {
        IsOpen = true;
        OnOpen.Invoke();
    }
    public void Open<T>(ModalParameters parameters = null, ModalOptions options = null) where T : class
    {
        modals.Add(new object());
        OnOpenComponent?.Invoke(typeof(T), parameters, options);

        
    }
    public Task<ModalResults> OpenAsync<T>(ModalParameters parameters = null, ModalOptions options = null) where T : class
    {
        TaskCompletionSource<ModalResults> taskCompletionSource = new();
        tasks.Add(taskCompletionSource);
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
        var modal = modals.LastOrDefault();
        if (modal != null)
        {
            IsOpen = false;
            OnClose(ModalResults.Empty(success));
            modals.Remove(modal);
        }
        
    }
    public void CloseSelf(ModalResults results = null)
    {
        Close(results);
    }
    public void Close(ModalResults result)
    {
        var modal = modals.LastOrDefault();
        //ModalResults result = new(success, parameters);
        if (modal != null)
        {
            IsOpen = false;
            OnClose(result);
            modals.Remove(modal);
        }
       
        TaskCompletionSource<ModalResults> taskCompletionSource = tasks.LastOrDefault();
        if (taskCompletionSource != null && taskCompletionSource.Task != null && !taskCompletionSource.Task.IsCompleted)
        {
            tasks.Remove(taskCompletionSource);
            taskCompletionSource.SetResult(result);
        }
    }
    private void OnClose(ModalResults results)
    {
        OnModalClose?.Invoke(results);
    }

}

public class ModalResults
{
    public ModalResults(bool success, ModalParameters parameters)
    {
        IsSucess = success;
        Parameters = parameters;
    }
    public bool IsSucess { get; private set; }
    public ModalParameters Parameters { get; private set; }

    public static ModalResults Empty(bool success)
    {
        return new(success, new ModalParameters());
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
    public Location Location { get; set; }
    public bool ShowCloseButton { get; set; }
    public string Height { get; set; } = "500px";
    public string Width { get; set; } = "500px";
    public bool CloseOnOuterClick { get; set; } = true;
}
