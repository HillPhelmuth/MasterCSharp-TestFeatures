using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using MasterCsharpHosted.Shared;
using Microsoft.AspNetCore.Components;

namespace MasterCsharpHosted.Client.Components
{
    public partial class OutputWindow : IDisposable
    {
        private bool _shouldRender;
        [Inject]
        private AppState AppState { get; set; }

        protected override Task OnInitializedAsync()
        {
            AppState.PropertyChanged += HandleAppStateStateChange;
            return base.OnInitializedAsync();
        }

        protected override Task OnAfterRenderAsync(bool firstRender)
        {
            _shouldRender = false;
            return base.OnAfterRenderAsync(firstRender);
        }

        protected override bool ShouldRender()
        {
            return _shouldRender;
        }
        private void HandleAppStateStateChange(object _, PropertyChangedEventArgs args)
        {
            if (args.PropertyName != nameof(AppState.CurrentOutput)) return;
            _shouldRender = true;
            StateHasChanged();
        }

        public void Dispose()
        {
            AppState.PropertyChanged -= HandleAppStateStateChange;
        }
    }
}
