using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MasterCsharpHosted.Shared.Services;
using Microsoft.AspNetCore.Components;

namespace MasterCsharpHosted.Client.Components
{
    public partial class SaveSnippetForm
    {
        [Parameter]
        public EventCallback<string> OnSave { get; set; }
        [Inject]
        private ModalService ModalService { get; set; }

        private string _description;
        private string _name;
        private string _errorText;

        private void SaveSnippet()
        {
            if (string.IsNullOrWhiteSpace(_name))
            {
                _errorText = "Name field is required";
                StateHasChanged();
                return;
            }
            string combinedValues = $"{_name}|{_description}";
            OnSave.InvokeAsync(combinedValues);
            var results = new ModalResults(true, new ModalParameters() {{"CombinedValues", combinedValues}});
            ModalService.Close(results);
        }

        private void InputHandler(ChangeEventArgs args)
        {
            if (string.IsNullOrWhiteSpace(_errorText)) return;
            if (!string.IsNullOrWhiteSpace(_name))
                _errorText = string.Empty;
        }
    }
}
