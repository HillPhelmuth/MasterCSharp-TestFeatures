using MasterCsharpHosted.Shared;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents
{
    public class AppComponentBase : ComponentBase, IDisposable
    {
        [Inject]
        protected AppState AppState { get; set; } = default!;

        protected override Task OnInitializedAsync()
        {
            AppState.PropertyChanged += UpdateState;
            return base.OnInitializedAsync();
        }
        protected virtual List<string> InterestingProperties => new();

        protected virtual void UpdateState(object? sender, PropertyChangedEventArgs e)
        {
            if (!InterestingProperties.Contains(e.PropertyName ?? "")) return;
            StateHasChanged();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            AppState.PropertyChanged -= UpdateState;
        }
    }
}
