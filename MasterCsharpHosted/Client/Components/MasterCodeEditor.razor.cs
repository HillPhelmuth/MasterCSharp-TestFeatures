using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using BlazorMonaco;
using BlazorMonaco.Editor;
using MasterCsharpHosted.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Range = BlazorMonaco.Range;

namespace MasterCsharpHosted.Client.Components
{
    public partial class MasterCodeEditor
    {
        [Inject]
        private ICodeClient PublicClient { get; set; }
        [Inject]
        private PublicClient TempClient { get; set; }
        [Inject]
        private AppState AppState { get; set; }
        
        [Parameter]
        public EventCallback<string> Submit { get; set; }
        [Parameter]
        public EventCallback<string> Save { get; set; }
        [Parameter]
        public EventCallback<string> Analyze { get; set; }
        [Parameter]
        public EventCallback<string> Explain { get; set; }
        [Parameter]
        public EventCallback ExplainBegin { get; set; }
        private StandaloneCodeEditor _editor = new();
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

        protected StandaloneEditorConstructionOptions EditorOptionsRoslyn(StandaloneCodeEditor editor)
        {
            return new()
            {
                AutomaticLayout = true,
                AutoIndent = "true",
                ColorDecorators = true,
                Minimap = new EditorMinimapOptions { Enabled = false },
                Hover = new EditorHoverOptions { Delay = 200 },
                Find = new EditorFindOptions
                { AutoFindInSelection = true, SeedSearchStringFromSelection = "true", AddExtraSpaceOnTop = true },
                Lightbulb = new EditorLightbulbOptions { Enabled = true },
                SuggestOnTriggerCharacters = true,
                Language = "csharp",
                GlyphMargin = true,
                Value = AppState.Snippet ?? CodeSnippets.DefaultCode
            };
        }

        private ActionDescriptor AddActionDescriptor(string id, string label, int[] keycodes, string precon,
            string context, string menuGroup, double menuOrder, Action<CodeEditor, int[]> action)
        {
            return new ActionDescriptor
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
            await _editor.SetValue(AppState.Snippet);
            await Global.SetTheme("vs-dark");

            await _editor.AddAction(AddActionDescriptor("executeAction", "Execute Code", new[] { (int)KeyMod.CtrlCmd| (int)KeyCode.Enter },
                null, null, "navigation", 1.5, ExecuteAction));
            await _editor.AddAction(AddActionDescriptor("Analyze", "Analyze Code", new[] { (int)KeyMod.CtrlCmd| (int)KeyCode.KeyA },
                null, null, "navigation", 2.0, ExecuteAction));
            await _editor.AddAction(AddActionDescriptor("Undo", "Undo", new[] { (int)KeyMod.CtrlCmd| (int)KeyCode.KeyZ }, null, null, "navigation", 3.5, ExecuteAction));
            await _editor.AddAction(AddActionDescriptor("Redo", "Redo", new[] { (int)KeyMod.CtrlCmd| (int)KeyCode.KeyY }, null, null, "navigation", 4.5, ExecuteAction));
            await _editor.AddAction(AddActionDescriptor("ZoomIn", "Zoom in", new[] { (int)KeyMod.CtrlCmd| (int)KeyCode.Period }, null, null, "navigation", 5.5, ExecuteAction));
            await _editor.AddAction(AddActionDescriptor("ZoomOut", "Zoom out", new[] { (int)KeyMod.CtrlCmd| (int)KeyCode.Comma }, null, null, "navigation", 6.5, ExecuteAction));
            await _editor.AddAction(AddActionDescriptor("Suggest", "Request Suggestion", new[] { (int)KeyMod.CtrlCmd| (int)KeyCode.KeyJ }, null, null, "navigation", 7.5, ExecuteAction));
            await _editor.AddAction(AddActionDescriptor("SignatureHelp", "Request Signature Help", new[] { (int)KeyMod.CtrlCmd| (int)KeyCode.KeyL }, null, null, "navigation", 8.5, ExecuteAction));
            if (!string.IsNullOrEmpty(AppState.CurrentUser?.UserName))
            {
                await _editor.AddAction(AddActionDescriptor("Save", "Save Code", new[] { (int)KeyMod.CtrlCmd| (int)KeyCode.KeyS }, null, null, "navigation", 9.5, ExecuteAction));
            }

            await _editor.AddAction(AddActionDescriptor("Decompile", "Compile and Decompile",
                new[] { (int)KeyMod.Alt| (int)KeyCode.KeyD },
                null, null, "navigation", 10.5, ExecuteAction));
            if (!string.IsNullOrEmpty(AppState.CurrentUser?.UserName))
            {
                await _editor.AddAction(AddActionDescriptor("Explain", "Ask GPT-3.5 to Explain", new[] { (int)KeyMod.CtrlCmd| (int)KeyCode.KeyE }, null, null, "navigation", 3.0, ExecuteAction));
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
            await ExplainBegin.InvokeAsync();
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
            
            await Explain.InvokeAsync(explanation);
        }
        private async Task ZoomInTask()
        {
            var editorOptions = new EditorUpdateOptions { FontSize = ++_fontSize };
            await _editor.UpdateOptions(editorOptions);
            Console.WriteLine("Tried Redo");
        }

        private async Task ZoomOutTask()
        {
            var editorOptions = new EditorUpdateOptions { FontSize = --_fontSize };
            await _editor.UpdateOptions(editorOptions);
            Console.WriteLine("Tried Redo");
        }

        private async void HandleExecute() => await SubmitCodeDefault();
        private async void ExecuteAction(CodeEditor ed = null, int[] keyCodes = null)
        {
            var keyTotal = keyCodes?.Sum();
            var editorAction = keyTotal switch
            {
                (int)KeyMod.CtrlCmd | (int)KeyCode.Enter => SubmitCodeDefault(),
                (int)KeyMod.CtrlCmd | (int)KeyCode.KeyJ => Suggest(),
                (int)KeyMod.CtrlCmd | (int)KeyCode.KeyA => SubmitForAnalysis(),
                (int)KeyMod.CtrlCmd | (int)KeyCode.KeyS => SaveTask(),
                (int)KeyMod.CtrlCmd | (int)KeyCode.Comma => ZoomOutTask(),
                (int)KeyMod.CtrlCmd | (int)KeyCode.Period => ZoomInTask(),
                (int)KeyMod.CtrlCmd | (int)KeyCode.KeyZ => Undo(),
                (int)KeyMod.CtrlCmd | (int)KeyCode.KeyY => Redo(),
                (int)KeyMod.CtrlCmd | (int)KeyCode.KeyL => SuggestSignature(),
                (int)KeyMod.CtrlCmd | (int)KeyCode.KeyE => RequestExplain(),
                (int)KeyMod.Alt | (int)KeyCode.KeyD => CompileAndDecompile(),
                _ => SubmitCodeDefault()
            };
            await editorAction;

        }

        private async Task SaveTask()
        {
            var code = await _editor.GetValue();
            await Save.InvokeAsync(code);
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
                        Range = new Range(pos.LineNumber, pos.Column, pos.LineNumber, lineEndCol),
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
            if (Submit.HasDelegate)
            {
                await Submit.InvokeAsync(code);
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
            await Analyze.InvokeAsync("Go");
        }

        private async Task SubmitForSimpleAnalysis(string code)
        {
            AppState.FullSyntaxTrees = await PublicClient.GetFullAnalysis(code);
            var settings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, PreserveReferencesHandling = PreserveReferencesHandling.All };
            AppState.TreeContent = JsonConvert.SerializeObject(AppState.FullSyntaxTrees, Formatting.Indented, settings);
            Console.WriteLine(AppState.TreeContent[..100]);
            await Analyze.InvokeAsync("Go");
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
                    await Global.SetTheme(AppState.EditorTheme);
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
