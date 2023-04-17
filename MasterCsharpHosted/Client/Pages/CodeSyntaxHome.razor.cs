using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using BlazorMonaco;
using BlazorMonaco.Editor;
using MasterCsharpHosted.Shared;
using Microsoft.AspNetCore.Components;

namespace MasterCsharpHosted.Client.Pages
{
    public partial class CodeSyntaxHome : IDisposable
    {
        [Inject]
        private AppState AppState { get; set; }
        [Inject]
        private ICodeClient PublicClient { get; set; }

        private bool _shouldRender;
        private bool _selfTrigger;
        private bool _isSimple;
        protected override Task OnInitializedAsync()
        {
            //AppState.OnShowCode += HandleCodeWindow;
            AppState.PropertyChanged += AppStateChanged;
            return base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                //AppState.SyntaxTreeInfo ??= await PublicClient.GetAnalysis(AppState.Snippet);
                await Task.Delay(250);
                await _editor.SetValue(AppState.Snippet);
            }
            System.Console.WriteLine("CodeSyntaxHome Rendered");
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
            //System.Console.WriteLine($"Received code from Diagram");
            _shouldRender = true;
            StateHasChanged();
        }
        private bool _showEdit = true;
        #region Editor
        private StandaloneCodeEditor _editor = new();
        protected StandaloneEditorConstructionOptions EditorOptionsSmall(CodeEditor editor)
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
        private ActionDescriptor AddActionDescriptor(string id, string label, int[] keycodes, string precon,
            string context, string menuGroup, double menuOrder, Action<BlazorMonaco.Editor.CodeEditor, int[]> action)
        {
            return new ActionDescriptor()
            {
                Id = id,
                Label = label,
                Keybindings = keycodes,
                Precondition = precon,
                ContextMenuGroupId = menuGroup,
                ContextMenuOrder = (float)menuOrder,
                KeybindingContext = context,
                Run = editor => action(editor, keycodes)
            };
        }
        protected async Task EditorOnDidInit()
        {
            await _editor.SetValue(AppState.SyntaxTreeInfo?.SourceCode ?? AppState.Snippet);
            await _editor.AddAction(AddActionDescriptor("Analyze", "Analyze Code", new[] { (int)KeyCode.Ctrl | (int)KeyCode.Enter },
                null, null, "navigation", 1.5,
                async (e, codes) =>
                {
                    await SubmitForAnalysis();
                }));
            
        }

        #endregion

        private async Task SubmitForAnalysis()
        {
            System.Console.WriteLine("SubmitForAnalysis triggered");
            _selfTrigger = true;
            var code = await _editor.GetValue();
            var analysisResult = await PublicClient.GetCodeAnalysis(code);
            AppState.SetAnalysisResults(analysisResult.SyntaxTree, analysisResult.FullSyntaxTrees);
            //AppState.SyntaxTreeInfo = await PublicClient.GetAnalysis(code);
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
        private void ToggleSimple()
        {
            _shouldRender = true;
            _isSimple = !_isSimple;
            StateHasChanged();
        }

        private void HandleCodeWindow(bool isShow = false)
        {
            _shouldRender = true;
            _showEdit = !_showEdit;
            StateHasChanged();
        }

        public void Dispose()
        {
            //AppState.OnShowCode -= HandleCodeWindow;
            AppState.PropertyChanged -= AppStateChanged;
        }
    }
}
