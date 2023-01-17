using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BlazorMonaco;
using MasterCsharpHosted.Shared;
using MasterCsharpHosted.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Range = BlazorMonaco.Range;

namespace MasterCsharpHosted.Client.Components
{
    public partial class CodeEditor : IDisposable
    {
        [Inject]
        private ICodeClient PublicClient { get; set; }
        [Inject]
        private PublicClient TempClient { get; set; }
        [Inject]
        private AppState AppState { get; set; }
        [Inject]
        private IJSRuntime Js { get; set; }

        [Parameter]
        public string CodeSnippet { get; set; }
        [Parameter]
        public EventCallback<string> OnSubmit { get; set; }
        [Parameter]
        public EventCallback<string> OnSave { get; set; }
        [Parameter]
        public EventCallback<string> OnAnalyze { get; set; }
        [Parameter]
        public EventCallback<string> OnExplain { get; set; }
        private MonacoEditor _editor = new();
        private string[] _deltaDecorationIds;
        private bool _shouldRender;
        private int _fontSize = 14;
        protected override Task OnInitializedAsync()
        {
            AppState.PropertyChanged += HandleAppStateStateChange;
            AppState.OnSubmitCode += HandleExecute;
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

        protected StandaloneEditorConstructionOptions EditorOptionsRoslyn(MonacoEditor editor)
        {
            return new()
            {
                AutomaticLayout = true,
                AutoIndent = true,
                ColorDecorators = true,
                Minimap = new EditorMinimapOptions { Enabled = false },
                Hover = new EditorHoverOptions { Delay = 200 },
                Find = new EditorFindOptions
                { AutoFindInSelection = true, SeedSearchStringFromSelection = true, AddExtraSpaceOnTop = true },
                Lightbulb = new EditorLightbulbOptions { Enabled = true },
                SuggestOnTriggerCharacters = true,
                Language = "csharp",
                GlyphMargin = true,
                Value = AppState.Snippet ?? CodeSnippets.DefaultCode
            };
        }

        protected async Task EditorOnDidInit(MonacoEditorBase editorBase)
        {
            await _editor.SetValue(AppState.Snippet);
            await MonacoEditorBase.SetTheme("vs-dark");
            await _editor.AddCommand((int)KeyMode.CtrlCmd | (int)KeyCode.KEY_H, (editor, keyCode) =>
            {
                Console.WriteLine("Ctrl+H : Initial editor command is triggered.");
            });
            await _editor.AddAction("executeAction", "Execute Code", new[] { (int)KeyMode.CtrlCmd | (int)KeyCode.Enter },
                null, null, "navigation", 1.5, ExecuteAction);
            await _editor.AddAction("Analyze", "Analyze Code", new[] { (int)KeyMode.CtrlCmd | (int)KeyCode.KEY_A },
                null, null, "navigation", 2.0, ExecuteAction);
            await _editor.AddAction("Undo", "Undo", new[] { (int)KeyMode.CtrlCmd | (int)KeyCode.KEY_Z },
                null, null, "navigation", 3.5, ExecuteAction);
            await _editor.AddAction("Redo", "Redo", new[] { (int)KeyMode.CtrlCmd | (int)KeyCode.KEY_Y },
                null, null, "navigation", 4.5, ExecuteAction);
            await _editor.AddAction("ZoomIn", "Zoom in", new[] { (int)KeyMode.CtrlCmd | (int)KeyCode.US_DOT },
                null, null, "navigation", 5.5, ExecuteAction);
            await _editor.AddAction("ZoomOut", "Zoom out", new[] { (int)KeyMode.CtrlCmd | (int)KeyCode.US_COMMA },
                null, null, "navigation", 6.5, ExecuteAction);
            await _editor.AddAction("Suggest", "Request Suggestion", new[] { (int)KeyMode.CtrlCmd | (int)KeyCode.KEY_J },
                null, null, "navigation", 7.5, ExecuteAction);
            await _editor.AddAction("SignatureHelp", "Request Signature Help", new[] { (int)KeyMode.CtrlCmd | (int)KeyCode.KEY_L },
                null, null, "navigation", 8.5, ExecuteAction);
            if (!string.IsNullOrEmpty(AppState.CurrentUser?.UserName))
            {
                await _editor.AddAction("Save", "Save Code", new[] { (int)KeyMode.CtrlCmd | (int)KeyCode.KEY_S }, null,
                    null, "navigation", 9.5, ExecuteAction);
            }

            await _editor.AddAction("Decompile", "Compile and Decompile",
                new[] {(int) KeyMode.CtrlCmd | (int) KeyCode.KEY_D},
                null, null, "navigation", 10.5, ExecuteAction);
            if (!string.IsNullOrEmpty(AppState.CurrentUser?.UserName))
            {
                await _editor.AddAction("Explain", "Ask for an explanation from OpenAI gpt-3",
                    new[] {(int) KeyMode.CtrlCmd | (int) KeyCode.KEY_E},
                    null, null, "navigation", 11.5, ExecuteAction);
            }
        }

        private async Task CompileAndDecompile()
        {
            var code = await _editor.GetValue();
            var decompiledCode = await TempClient.CompileAndDecompile(code);
            await _editor.SetValue(decompiledCode);
            StateHasChanged();
        }

        private async Task RequestExplain()
        {
            var code = await _editor.GetValue();
            var model = await _editor.GetModel();
            var selection = await _editor.GetSelection();
            var selectedRange = new Range(selection.StartLineNumber, selection.SelectionStartColumn, selection.EndLineNumber,
                selection.EndColumn);
            Console.WriteLine($"SelectedRange:\n{JsonConvert.SerializeObject(selectedRange)}");
            var text = await model.GetValueInRange(selectedRange, EndOfLinePreference.TextDefined);
            Console.WriteLine($"Selected Text:\n{text}");
            var requestText = string.IsNullOrWhiteSpace(text) ? code : text;
            var explanation = await TempClient.GetExplanation(requestText);
            await OnExplain.InvokeAsync(explanation);
        }
        private async Task ZoomInTask()
        {
            var editorOptions = new GlobalEditorOptions { FontSize = ++_fontSize };
            await _editor.UpdateOptions(editorOptions);
            Console.WriteLine("Tried Redo");
        }

        private async Task ZoomOutTask()
        {
            var editorOptions = new GlobalEditorOptions { FontSize = --_fontSize };
            await _editor.UpdateOptions(editorOptions);
            Console.WriteLine("Tried Redo");
        }

        private async void HandleExecute() => await SubmitCodeDefault();
        private async void ExecuteAction(MonacoEditorBase ed = null, int[] keyCodes = null)
        {
            var keyTotal = keyCodes?.Sum();
            var editorAction = keyTotal switch
            {
                (int)KeyMode.CtrlCmd | (int)KeyCode.Enter => SubmitCodeDefault(),
                (int)KeyMode.CtrlCmd | (int)KeyCode.KEY_J => Suggest(),
                (int)KeyMode.CtrlCmd | (int)KeyCode.KEY_A => SubmitForAnalysis(),
                (int)KeyMode.CtrlCmd | (int)KeyCode.KEY_S => SaveTask(),
                (int)KeyMode.CtrlCmd | (int)KeyCode.US_COMMA => ZoomOutTask(),
                (int)KeyMode.CtrlCmd | (int)KeyCode.US_DOT => ZoomInTask(),
                (int)KeyMode.CtrlCmd | (int)KeyCode.KEY_Z => Undo(),
                (int)KeyMode.CtrlCmd | (int)KeyCode.KEY_Y => Redo(),
                (int)KeyMode.CtrlCmd | (int)KeyCode.KEY_L => SuggestSignature(),
                (int)KeyMode.CtrlCmd | (int)KeyCode.KEY_E => RequestExplain(),
                (int)KeyMode.CtrlCmd | (int)KeyCode.KEY_D => CompileAndDecompile(),
                _ => SubmitCodeDefault()
            };
            await editorAction;

        }

        private async Task SaveTask()
        {
            var code = await _editor.GetValue();
            await OnSave.InvokeAsync(code);
        }

        private async void EditorDidChangeCursorPosition(CursorPositionChangedEvent eventArgs)
        {
            var model = await _editor.GetModel();
            if (eventArgs == null || model == null) return;
            var word = await model.GetWordAtPosition(eventArgs.Position);

            var pos = eventArgs.Position;
            var lineEndCol = await model.GetLineLastNonWhitespaceColumn(pos.LineNumber);
            var lineContent = await model.GetLineContent(pos.LineNumber);
            Console.WriteLine($"Word at current cursor: {word?.Word}");
            var deltas = _deltaDecorationIds;
            var listDelta = new List<ModelDeltaDecoration>();

            foreach (var hoverContent in AppHoverContent.AllAppHoverItems)
            {
                foreach (var (key, messages) in hoverContent.KeyWordMessages.Where(x => lineContent.Contains(x.Key)))
                {
                    var decoration = new ModelDeltaDecoration
                    {
                        Range = new BlazorMonaco.Range(pos.LineNumber, pos.Column, pos.LineNumber, lineEndCol),
                        Options = new ModelDecorationOptions
                        {
                            IsWholeLine = true,
                            GlyphMarginClassName = "decorationGlyphMargin",
                            GlyphMarginHoverMessage = messages.Select(msg => new MarkdownString { Value = msg, IsTrusted = true }).ToArray(),
                            ZIndex = hoverContent.ZIndex,
                            ClassName = "decorationContentClass",
                            InlineClassName = "decorationContentClass"
                        }
                    };
                    listDelta.Add(decoration);
                }
            }
            _deltaDecorationIds = await _editor.DeltaDecorations(deltas, listDelta.ToArray());
        }

        private async Task SubmitCodeDefault()
        {
            var code = await _editor.GetValue();
            if (OnSubmit.HasDelegate)
            {
                await OnSubmit.InvokeAsync(code);
                return;
            }

            var returnedValue = await PublicClient.CompileCodeAsync(code);
            AppState.AddLineToOutput(returnedValue);
        }

        private async Task SubmitForAnalysis()
        {
            var code = await _editor.GetValue();
            var result = await PublicClient.GetCodeAnalysis(code);
            AppState.SetAnalysisResults(result.SyntaxTree, result.FullSyntaxTrees);
            AppState.Snippet = code;
            await OnAnalyze.InvokeAsync("Go");
        }

        private async Task SubmitForSimpleAnalysis(string code)
        {
            AppState.FullSyntaxTrees = await PublicClient.GetFullAnalysis(code);
            var settings = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, PreserveReferencesHandling = PreserveReferencesHandling.All };
            AppState.TreeContent = JsonConvert.SerializeObject(AppState.FullSyntaxTrees, Formatting.Indented, settings);
            Console.WriteLine(AppState.TreeContent[..100]);
            await OnAnalyze.InvokeAsync("Go");
        }
        private async Task Undo() => await _editor.Trigger("whatever...", "undo", "whatever...");
        private async Task Redo() => await _editor.Trigger("whatever...", "redo", "whatever...");
        private async Task Suggest() => await _editor.Trigger("whatever...", "editor.action.triggerSuggest", "whatever...");

        private async Task SuggestSignature() => await _editor.Trigger("whatever...", "editor.action.triggerParameterHints", "whatever...");
        private async void HandleAppStateStateChange(object _, PropertyChangedEventArgs args)
        {
            Console.WriteLine($"Editor Handles AppState change of {args.PropertyName} property");
            _shouldRender = true;
            switch (args.PropertyName)
            {
                case nameof(AppState.SyntaxTreeInfo):
                    StateHasChanged();
                    return;
                case nameof(AppState.FullSyntaxTrees):
                    StateHasChanged();
                    return;
                case nameof(AppState.EditorTheme):
                    await MonacoEditorBase.SetTheme(AppState.EditorTheme);
                    StateHasChanged();
                    Console.WriteLine("Theme set to " + AppState.EditorTheme);
                    return;
                case nameof(AppState.Snippet):
                    await _editor.SetValue(AppState.Snippet);
                    StateHasChanged();
                    break;
                case nameof(AppState.TreeContent):
                    StateHasChanged();
                    break;
                default:
                    _shouldRender = false;
                    return;
            }
        }

        public void Dispose()
        {
            AppState.PropertyChanged -= HandleAppStateStateChange;
            AppState.OnSubmitCode -= HandleExecute;
        }
    }
}
