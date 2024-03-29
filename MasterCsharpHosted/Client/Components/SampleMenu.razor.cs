﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MasterCsharpHosted.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;

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
        [Parameter]
        public EventCallback<UserSnippet> OnDeleteSnippet { get; set; }
        
        
        private void MouseOverContent(string content)
        {
            AppState.Content = content;
            System.Threading.Tasks.Parallel.Invoke(
                () => PrintNumbers(1, 5),
                () => PrintNumbers(6, 10)
            );
        }
        public static void PrintNumbers(int start, int end)
        {
            for (int i = start; i <= end; i++)
            {
                Console.WriteLine($"Number {i}");
            }
        }
        private async Task GetCodeFromGithub(CodeSample snippet)
        {
            string code = await PublicClient.GetFromGithubRepo(snippet.Code);
            MouseOverContent(snippet.Description);
            AppState.UpdateSnippet(code);
        }
    }
}
