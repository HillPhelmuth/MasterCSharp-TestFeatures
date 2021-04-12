using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MasterCsharpHosted.Shared;
using Microsoft.AspNetCore.Components;

namespace MasterCsharpHosted.Client.Pages
{
    public partial class ChallengeHome
    {
        [Inject]
        private AppState AppState { get; set; }
        [Inject]
        private PublicClient PublicClient { get; set; }

        private CodeOutputModel results;
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
        private void HandleSubmit(string code)
        {
            _isWorking = true;
            StateHasChanged();
            _ = Submit(code);
        }
        private async Task Submit(string code)
        {
            var submitChallenge = new ChallengeModel
            {
                Solution = code,
                Tests = AppState.ActiveChallenge.Tests
            };
            var output = await PublicClient.SubmitChallenge(submitChallenge);
            foreach (string result in output.Outputs.Select(x => x.Codeout))
            {
                AppState.AddLineToOutput(result);
            }
            foreach (var result in output.Outputs)
            {
                System.Console.WriteLine($"test: {result.TestIndex}, result: {result.TestResult}, against: {result.Test.TestAgainst}, output: {result.Codeout}");
            }
            bool challengeSucceed = output.Outputs.All(x => x.TestResult);
            _testResult = challengeSucceed ? TestResult.Pass : TestResult.Fail;
            //var debugString = challengeSucceed ? "True" : "False";
            //System.Console.WriteLine($"isChallengeSucceed = {debugString}");
            //_isChallengeFail = !challengeSucceed;
            //System.Console.WriteLine($"isChallengeFail = {_isChallengeFail}");
            _isWorking = false;
            results = output;
            await InvokeAsync(StateHasChanged);
        }
    }
}
