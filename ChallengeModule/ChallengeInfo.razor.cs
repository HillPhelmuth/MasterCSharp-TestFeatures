using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MasterCsharpHosted.Shared;
using Microsoft.AspNetCore.Components;

namespace ChallengeModule
{
    public partial class ChallengeInfo : IDisposable
    {
        [Inject]
        private AppState AppState { get; set; }
        [Parameter]
        public EventCallback<bool> OnIsReady { get; set; }
        //private ChallengeModel _challenge = new();


        protected override Task OnInitializedAsync()
        {
            AppState.PropertyChanged += HandleAppStateStateChange;
            return base.OnInitializedAsync();
        }

        private void Ready() => OnIsReady.InvokeAsync(true);
        private void HandleAppStateStateChange(object _, PropertyChangedEventArgs args)
        {
            if (args.PropertyName != nameof(AppState.ActiveChallenge)) return;
            //_challenge = AppState.ActiveChallenge;
            StateHasChanged();
        }

        public void Dispose()
        {
            AppState.PropertyChanged -= HandleAppStateStateChange;
        }
    }
}
