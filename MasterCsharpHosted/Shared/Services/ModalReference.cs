using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterCsharpHosted.Shared.Services
{
    internal class ModalReference
    {
        public ModalReference()
        {
            Id = Guid.NewGuid();
        }
        internal Guid Id { get; }
        internal TaskCompletionSource<ModalResults> TaskCompletionSource { get; set; }
    }
    public class ModalResults
    {
        public ModalResults(bool success, ModalParameters parameters)
        {
            IsSuccess = success;
            Parameters = parameters;
        }
        public bool IsSuccess { get; }
        public ModalParameters Parameters { get; }

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
        public bool HideCloseButton { get; set; }
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
}
