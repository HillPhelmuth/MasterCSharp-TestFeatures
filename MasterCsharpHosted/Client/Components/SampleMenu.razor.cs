using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MasterCsharpHosted.Shared;
using Microsoft.AspNetCore.Components;

namespace MasterCsharpHosted.Client.Components
{
    public partial class SampleMenu
    {
        [Inject]
        private AppState AppState { get; set; }
        [Inject]
        private IPublicClient PublicClient { get; set; }
        [Parameter]
        public EventCallback<bool> OnCloseMenu { get; set; }

        private void MouseOverContent(string content)
        {
            AppState.Content = content;
        }

        private async Task GetCodeFromGithub(CodeSample snippet)
        {
            var code = await PublicClient.GetFromGithubRepo(snippet.Code);
            MouseOverContent(snippet.Description);
            AppState.UpdateSnippet(code);
        }
    }
}
