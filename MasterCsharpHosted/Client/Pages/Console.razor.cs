using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using MasterCsharpHosted.Client.Components;
using MasterCsharpHosted.Shared;
using MasterCsharpHosted.Shared.Services;
using Microsoft.AspNetCore.Components;
using SharedComponents;

namespace MasterCsharpHosted.Client.Pages
{
    public partial class Console : AppComponentBase/*, IDisposable*/
    {
        
        [Inject]
        private PublicClient PublicClient { get; set; }
        [Inject]
        private ModalService ModalService { get; set; }
        private string _cssClass = "inactive";
        private bool _isMenuOpen;
        private readonly List<string> _themeOptions = new() {"vs", "vs-dark", "hc-black"};
       
        protected override Task OnInitializedAsync()
        {
            //AppState.PropertyChanged += HandleAppStateStateChange;
            return base.OnInitializedAsync();
        }

        private async Task OpenMenu()
        {
            _cssClass = "active";
            StateHasChanged();
            await Task.Delay(500);
            _isMenuOpen = true;
        }
        private void CloseMenu()
        {
            if (!_isMenuOpen) return;
            _cssClass = "inactive";
            _isMenuOpen = false;
            StateHasChanged();
        }

        private string _snippetCode;
        private async void SaveSnippet(string code)
        {
            _snippetCode = code;
            var modalResult =
                await ModalService.OpenAsync<SaveSnippetForm>(options: new ModalOptions {Location = Location.TopRight});
            if (modalResult is not {IsSuccess: true}) return;
            var saveData = modalResult.Parameters?.Get<string>("CombinedValues");
            if (string.IsNullOrEmpty(saveData)) return;
            HandleSave(saveData);
        }

        private void HandleSave(string snippetData)
        {
            var splitData = snippetData.Split('|');
            var name = splitData[0];
            var description = splitData[1];

            var snippet = new UserSnippet(name, _snippetCode, description);
            if (AppState.CurrentUser.Snippets.Any(x => x.Name == name))
            {
                snippet = AppState.CurrentUser.Snippets.FirstOrDefault(x => x.Name == name) ?? new UserSnippet();
                snippet.Code = _snippetCode;
                snippet.Description = description;
            }
            else
            {
                AppState.CurrentUser.Snippets.Add(snippet);
            }

            _ = PublicClient.UpdateUser(AppState.CurrentUser);
            StateHasChanged();

        }

        private void HandleThemeChange(string theme)
        {
            //AppState.EditorTheme = theme;
            //StateHasChanged();
        }
        //private void HandleAnalyze(string syntaxOjb)
        //{
        //    StateHasChanged();
        //}

        private async void ShowGetCodeFromGithub()
        {
            var modalOptions = new ModalOptions {Location = Location.TopLeft, Width = "30rem"};
            var requestContent = await ModalService.OpenAsync<GitHubForm>(options: modalOptions);
            if (requestContent is {IsSuccess: true})
            {
                var org = requestContent.Parameters?.Get<string>("Organization") ?? "";
                var repo = requestContent.Parameters?.Get<string>("Repo") ?? "";
                var path = requestContent.Parameters?.Get<string>("FullPath") ?? "";
                AppState.Snippet = await PublicClient.GetFromPublicRepo(org, repo, path);
            }
            StateHasChanged();
        }

        protected override List<string> InterestingProperties => new() {nameof(AppState.Content)};

        //private void HandleAppStateStateChange(object _, PropertyChangedEventArgs args)
        //{
        //    if (args.PropertyName != nameof(AppState.Content)) return;
        //    StateHasChanged();
        //}

        //public void Dispose()
        //{
        //    AppState.PropertyChanged -= HandleAppStateStateChange;
        //}
    }
}
