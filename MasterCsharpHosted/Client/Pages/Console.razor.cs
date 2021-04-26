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
        [Inject]
        private PublicClient PublicClient { get; set; }
        private string cssClass = "inactive";
        private bool _isMenuOpen;
        private bool _showModal;
        private bool _showSaveModal;
        private string _syntax;

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

        private string _snippetCode;
        private void SaveSnippet(string code)
        {
            _snippetCode = code;
            _showSaveModal = true;
        }

        private void HandleSave(string snippetData)
        {
            var splitData = snippetData.Split('|');
            var name = splitData[0];
            var description = splitData[1];

            var snippet = new UserSnippet(name, _snippetCode, description);
            if (AppState.CurrentUser.Snippets.Any(x => x.Name == name))
            {
                snippet = AppState.CurrentUser.Snippets.FirstOrDefault(x => x.Name == name);
                snippet.Code = _snippetCode;
                snippet.Description = description;
            }
            else
            {
                AppState.CurrentUser.Snippets.Add(snippet);
            }

            _ = PublicClient.UpdateUser(AppState.CurrentUser);
            _showSaveModal = false;
            StateHasChanged();

        }

        private void HandleAnalyze(string syntaxOjb)
        {
            _syntax = syntaxOjb;
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
