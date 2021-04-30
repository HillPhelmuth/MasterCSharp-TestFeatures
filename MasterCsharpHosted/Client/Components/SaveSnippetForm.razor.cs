using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace MasterCsharpHosted.Client.Components
{
    public partial class SaveSnippetForm
    {
        [Parameter]
        public EventCallback<string> OnSave { get; set; }

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
        }

        private void InputHandler(ChangeEventArgs args)
        {
            if (string.IsNullOrWhiteSpace(_errorText)) return;
            if (!string.IsNullOrWhiteSpace(_name))
                _errorText = string.Empty;
        }
    }
}
