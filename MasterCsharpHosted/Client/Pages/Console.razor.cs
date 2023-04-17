using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MasterCsharpHosted.Client.Components;
using MasterCsharpHosted.Shared;
using MasterCsharpHosted.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using SharedComponents;

namespace MasterCsharpHosted.Client.Pages
{
    public partial class Console : AppComponentBase/*, IDisposable*/
    {

        [Inject]
        private ICodeClient CodeClient { get; set; }
        [Inject]
        private IUserClient UserClient { get; set; }
        [Inject]
        private ModalService ModalService { get; set; }

        private string _cssClass = "inactive";
        private bool _isMenuOpen;
        private readonly List<string> _themeOptions = new() { "vs", "vs-dark", "hc-black" };

        protected override Task OnInitializedAsync()
        {
            //AppState.PropertyChanged += HandleAppStateStateChange;
            return base.OnInitializedAsync();
        }
        [Inject]
        private PublicClient Client { get; set; }
        private async void UploadFile(InputFileChangeEventArgs args)
        {
            var file = args.File;
            if (!file.Name.EndsWith(".dll")) return;
            var fileContent =
                new StreamContent(file.OpenReadStream());
            fileContent.Headers.ContentType =
                new MediaTypeHeaderValue(file.ContentType);
            var streamContent = file.OpenReadStream();
            using var memStream = new MemoryStream();
            await streamContent.CopyToAsync(memStream);
            var httpClient = Client.Client;
            var response = await httpClient.PostAsJsonAsync("api/code/decompileFile", memStream.ToArray());
            var content = await response.Content.ReadAsStringAsync();
            AppState.Snippet = content;
            StateHasChanged();
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
                await ModalService.OpenAsync<SaveSnippetForm>(options: new ModalOptions { Location = Location.TopRight });
            if (modalResult is not { IsSuccess: true }) return;
            var saveData = modalResult.Parameters?.Get<string>("CombinedValues");
            if (string.IsNullOrEmpty(saveData)) return;
            HandleSave(saveData);
        }

        private async void HandleDeleteSnippet(UserSnippet snippet)
        {
            var confirm = await ModalService.ConfirmAsync(new ModalConfirmOptions($"Delete Snippet ({snippet.Name})")
            {
                ConfirmButton = new ConfirmButton("Yep, ditch it"),
                DeclineButton = new DeclineButton("Noooo!!!")
            });
            if (!confirm) return;
            AppState.DeleteUserSnippet(snippet);
            await UserClient.UpdateUser(AppState.CurrentUser);
            StateHasChanged();

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

            _ = UserClient.UpdateUser(AppState.CurrentUser);
            StateHasChanged();

        }

        private bool _isOpen;

        private void HandleExplainRequest()
        {
            _isOpen = true;
            StateHasChanged();
        }
        private async void HandleExplain(string explain)
        {
            _isOpen = false;
            System.Console.WriteLine($"Explanation provided: {explain}");
            var modalparams = new ModalParameters() { { "Content", explain } };
            var modalOptions = new ModalOptions() { CloseOnOuterClick = true, Title = "GPT-3 Explanation" };
            
            await ModalService.OpenAsync<ExplanationView>(modalparams, modalOptions);
            
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
            var modalOptions = new ModalOptions { Location = Location.TopLeft, Width = "30rem" };
            var requestContent = await ModalService.OpenAsync<GitHubForm>(options: modalOptions);
            if (requestContent is { IsSuccess: true })
            {
                var org = requestContent.Parameters?.Get<string>("Organization") ?? "";
                var repo = requestContent.Parameters?.Get<string>("Repo") ?? "";
                var path = requestContent.Parameters?.Get<string>("FullPath") ?? "";
                AppState.Snippet = await CodeClient.GetFromPublicRepo(org, repo, path);
            }
            StateHasChanged();
        }

        protected override List<string> InterestingProperties => new() { nameof(AppState.Content) };

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
