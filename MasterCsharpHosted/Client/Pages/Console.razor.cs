using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using MasterCsharpHosted.Shared;
using Microsoft.AspNetCore.Components;

namespace MasterCsharpHosted.Client.Pages
{
    public partial class Console : IDisposable
    {
        [Inject]
        private AppState AppState { get; set; }
        private string cssClass = "inactive";

        protected override Task OnInitializedAsync()
        {
            AppState.PropertyChanged += HandleAppStateStateChange;
            return base.OnInitializedAsync();
        }

        private void OpenMenu()
        {
            cssClass = "active";
            StateHasChanged();
        }

        private void CloseMenu(bool isClose)
        {
            cssClass = "inactive";
            StateHasChanged();
        }

        private void HandleAppStateStateChange(object _, PropertyChangedEventArgs args)
        {
            if (args.PropertyName != nameof(AppState.Content)) return;
            StateHasChanged();
        }

        public void Dispose()
        {
            AppState.PropertyChanged -= HandleAppStateStateChange;
        }
    }
}
