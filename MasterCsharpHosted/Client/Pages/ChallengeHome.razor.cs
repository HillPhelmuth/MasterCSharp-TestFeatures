using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChallengeModule;
using MasterCsharpHosted.Shared;
using MasterCsharpHosted.Shared.Services;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;

namespace MasterCsharpHosted.Client.Pages
{
    public partial class ChallengeHome
    {
        [Inject]
        private AppState AppState { get; set; }
        [Inject]
        private IChallengeClient PublicClient { get; set; }
        [Inject]
        private IUserClient UserClient { get; set; }
        [Inject]
        private ModalService ModalService { get; set; }

        private TestResult _testResult;
        private bool _isReady;
        private bool _isMenuOpen = true;
        private bool _isWorking;
        private bool _showOutput;
        private string _cssClass = "active";
        

        protected override Task OnInitializedAsync()
        {

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
        
        private async Task Submit(string code)
        {
            _isWorking = true;
            StateHasChanged();
            await Task.Delay(1000);
            var submitChallenge = new ChallengeModel
            {
                Solution = code,
                Tests = AppState.ActiveChallenge.Tests
            };
            var output = await PublicClient.SubmitChallenge(submitChallenge);
            _isWorking = false;
            await Task.Delay(1);
            StateHasChanged();
            AppState.ChallengeOutput = output;
            foreach (string result in output.Outputs.Select(x => x.Codeout))
            {
                AppState.AddLineToOutput(result);
            }
            foreach (var result in output.Outputs)
            {
                System.Console.WriteLine($"test: {result.TestIndex}, result: {result.TestResult}, against: {result.Test.TestAgainst}, output: {result.Codeout}");
            }
            var modalTitle = output.PassAll ? "You succeeded! Woo hoo!" : "You failed! Boooooo!";
            await ModalService.OpenAsync<ResultTable>(options:new ModalOptions { Title = modalTitle, Height= "50vh", Width = "40vw"});
            
            if (output.PassAll)
                await TryAddChallengeToUser(code);
            
        }
        private async void ShowResults()
        {
            if (AppState.ChallengeOutput == null) return;
            await ModalService.OpenAsync<ResultTable>(options: new ModalOptions { Title = "Challenge Results" });
        }
        private void ShowSolution()
        {
            var snippet = AppState.CurrentUser.CompletedChallenges.FirstOrDefault(x => x.ChallengeName == AppState.ActiveChallenge.Name)?.Solution;
            if (string.IsNullOrEmpty(snippet)) return;
            AppState.UpdateSnippet(snippet);
        }
        private async Task<bool> TryAddChallengeToUser(string code)
        {
            System.Console.WriteLine("TryAddChallengeToUser() executed");
            bool submitSuccess = false;
            if (AppState.CurrentUser.IsAuthenticated && (AppState.CurrentUser.CompletedChallenges == null || AppState.CurrentUser.CompletedChallenges?.Any(x => x.ChallengeName == AppState.ActiveChallenge.Name) == false))
            {
                AppState.CurrentUser.CompletedChallenges.Add(new CompletedChallenge(AppState.ActiveChallenge.Name, code));
                submitSuccess = await UserClient.UpdateUser(AppState.CurrentUser);
            }
            System.Console.WriteLine($"TryAddChallengeToUser() Completed. Success: {submitSuccess}");
            return submitSuccess;
        }
    }
}
