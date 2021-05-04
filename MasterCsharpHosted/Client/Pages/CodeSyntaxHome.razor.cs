using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using BlazorMonaco;
using MasterCsharpHosted.Shared;
using Microsoft.AspNetCore.Components;

namespace MasterCsharpHosted.Client.Pages
{
    public partial class CodeSyntaxHome
    {
        [Inject]
        private AppState AppState { get; set; }
        [Inject]
        private ICodeClient PublicClient { get; set; }

        private bool _shouldRender;
        private bool _selfTrigger;
        protected override Task OnInitializedAsync()
        {
            AppState.PropertyChanged += AppStateChanged;
            return base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                AppState.SyntaxTreeInfo ??= await PublicClient.GetAnalysis(AppState.Snippet);
                await Task.Delay(250);
                await _editor.SetValue(AppState.Snippet);
            }
            _shouldRender = false;
           await base.OnAfterRenderAsync(firstRender);
        }
        
        protected override bool ShouldRender()
        {
            return _shouldRender;
        }

        private async void HandleSendCode(string code)
        {
            await _editor.SetValue(code);
            System.Console.WriteLine($"Received code from Diagram:\n{code.Substring(0, Math.Min(150, code.Length))}...");
            _shouldRender = true;
            StateHasChanged();
        }
        #region Editor
        private MonacoEditor _editor = new();
        protected StandaloneEditorConstructionOptions EditorOptionsSmall(MonacoEditor editor)
        {
            return new()
            {
                Minimap = new EditorMinimapOptions { Enabled = false },
                Language = "csharp",
                Theme = "vs-dark",
                GlyphMargin = false,
                FontSize = 13,
                LineNumbers = "off",
                Value = AppState.SyntaxTreeInfo?.SourceCode ?? AppState.Snippet ?? CodeSnippets.DefaultCode

            };
        }

        protected async Task EditorOnDidInit(MonacoEditorBase editorBase)
        {
            await _editor.SetValue(AppState.SyntaxTreeInfo?.SourceCode ?? AppState.Snippet);
            await _editor.AddAction("Analyze", "Analyze Code", new[] { (int)KeyMode.CtrlCmd | (int)KeyCode.Enter },
                null, null, "navigation", 1.5,
                async (e, codes) =>
                {
                    await SubmitForAnalysis();
                });
        }

        #endregion

        private async Task SubmitForAnalysis()
        {
            _selfTrigger = true;
            string code = await _editor.GetValue();
            AppState.SyntaxTreeInfo = await PublicClient.GetAnalysis(code);
            AppState.Snippet = code;
        }
        private async void AppStateChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(AppState.SyntaxTreeInfo) && e.PropertyName != nameof(AppState.Snippet)) return;
            _shouldRender = !_selfTrigger;
            _selfTrigger = false;
            if (_shouldRender)
                await InvokeAsync(StateHasChanged);
        }
    }
}
