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
        private bool _isMenuOpen;
        private bool _showGithubForm;
        private bool _showContent;
        private bool _showModal;
        protected override Task OnInitializedAsync()
        {
            AppState.PropertyChanged += HandleAppStateStateChange;
            return base.OnInitializedAsync();
        }

        private void CloseModal(bool closed)
        {
            _showModal = false;
            //StateHasChanged();
        }
        private async Task OpenMenu()
        {
            cssClass = "active";
            StateHasChanged();
            await Task.Delay(500);
            _isMenuOpen = true;
        }
        private void CloseMenu()
        {
            if (!_isMenuOpen) return;
            cssClass = "inactive";
            _isMenuOpen = false;
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
