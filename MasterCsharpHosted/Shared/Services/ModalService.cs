using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterCsharpHosted.Shared.Services
{
    public class ModalService
    {
        public bool IsOpen { get; set; }
        public ModalParameters Parameters { get; set; }
        public event Func<ModalResults, Task> OnModalClose;
        

        public void Close(bool success)
        {
            IsOpen = false;
            OnClose(ModalResults.Empty(success));
        }

        public void Close(bool success, ModalParameters parameters)
        {
            IsOpen = false;
            OnClose(new ModalResults(success, parameters));
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
                return (T) parameterValue;
            parameterValue = defaultValue;
            this[parameterName] = parameterValue;
            return (T)parameterValue;
        }
        public void Set<T>(string parameterName, T parameterValue)
        {
            this[parameterName] = parameterValue;
        }
    }
}
