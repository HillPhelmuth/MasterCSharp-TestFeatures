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
    private readonly Stack<ModalReference> _modalReferences = new();
    private readonly List<object> _modals = new();
    public event Action<ModalResults> OnModalClose;
    
    public event Action<Type, ModalParameters, ModalOptions> OnOpenComponent;
    public event Action<RenderFragment<ModalService>, ModalParameters, ModalOptions> OnOpenFragment;
    public event Action<ModalConfirmOptions> OnConfirmOpen;
    public event Action<bool> OnModalConfirmClose;
   

    public Task<bool> ConfirmAsync(ModalConfirmOptions options)
    {
        TaskCompletionSource<bool> taskCompletionSource = new();
        _confirmTasks.Add(taskCompletionSource);
        _modals.Add(new object());
        OnConfirmOpen?.Invoke(options);
        return taskCompletionSource.Task;
    }
    public void Open<T>(ModalParameters parameters = null, ModalOptions options = null) where T : ComponentBase
    {
        _modalReferences.Push(new ModalReference());
        OnOpenComponent?.Invoke(typeof(T), parameters, options);
    }

    public void Open(RenderFragment<ModalService> renderFragment, ModalParameters parameters = null, ModalOptions options = null)
    {
        _modals.Add(new object());
        OnOpenFragment?.Invoke(renderFragment, parameters, options);
    }
    public Task<ModalResults> OpenAsync<T>(ModalParameters parameters = null, ModalOptions options = null) where T : ComponentBase
    {
        TaskCompletionSource<ModalResults> taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        _tasks.Add(taskCompletionSource);
        var modalReference = new ModalReference(){TaskCompletionSource = taskCompletionSource};
        _modalReferences.Push(modalReference);
        OnOpenComponent?.Invoke(typeof(T), parameters, options);
        return modalReference.TaskCompletionSource.Task;

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
        //IsOpen = false;
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
            //IsOpen = false;
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
        if (!_modalReferences.TryPop(out var modalRef)) return;
        //IsOpen = false;
        OnClose(result);
        var task = modalRef.TaskCompletionSource;
        if (task == null || task.Task.IsCompleted) return;
        task.SetResult(result);
        //var modal = _modals.LastOrDefault();
        ////ModalResults result = new(success, parameters);
        //if (modal != null)
        //{
        //    IsOpen = false;
        //    OnClose(result);
        //    _modals.Remove(modal);
        //}

        //var taskCompletionSource = _tasks.LastOrDefault();
        //if (taskCompletionSource == null || taskCompletionSource.Task.IsCompleted) return;
        //_tasks.Remove(taskCompletionSource);
        //taskCompletionSource.SetResult(result);
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
