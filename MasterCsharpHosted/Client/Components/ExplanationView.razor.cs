using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using MasterCsharpHosted.Shared;
using Microsoft.AspNetCore.Components;

namespace MasterCsharpHosted.Client.Components
{
    public partial class ExplanationView
    {
        [Parameter] public string Content { get; set; } = "";

        [Inject] private AppState AppState { get; set; } = default!;

        private List<string> _contentItems = new();
        protected override Task OnInitializedAsync()
        {
            AppState.PropertyChanged += UpdateState;
            return base.OnInitializedAsync();
        }
        protected override Task OnParametersSetAsync()
        {
            if (string.IsNullOrEmpty(Content) || !Content.Contains("//")) return base.OnParametersSetAsync();
            var items = Content.Split("//");
            _contentItems = items.ToList();
            return base.OnParametersSetAsync();
        }

        private async void UpdateState(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName != nameof(AppState.ExplainContent)) return;
            Content += AppState.Content;
            StateHasChanged();

        }

        private static string AsHtml(string content)
        {
            return content.Replace("\r\n", "<br/>").Replace("\n", "<br/>");
        }
    }
}
