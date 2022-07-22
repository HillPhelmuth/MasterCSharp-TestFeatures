using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using BlazorMonaco;
using MasterCsharpHosted.Shared;
using MasterCsharpHosted.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace MasterCsharpHosted.Client.Components
{
    public partial class CodeEditor : IDisposable
    {
        [Inject]
        private ICodeClient PublicClient { get; set; }
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
        private MonacoEditor _editor = new();
        private string[] _deltaDecorationIds;
        private bool _shouldRender;
        private int _fontSize = 14;
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
                //Theme = "vs-dark",
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
            await _editor.AddAction("executeAction", "Execute Code",
                new[] { (int)KeyMode.CtrlCmd | (int)KeyCode.Enter }, null, null, "navigation", 1.5,
                async (ed, keyCodes) =>
                {
                    await SubmitCodeDefault();
                    Console.WriteLine("Code Executed from Editor Command");
                });
            await _editor.AddAction("Analyze", "Analyze Code", new[] { (int)KeyMode.CtrlCmd | (int)KeyCode.KEY_A },
                null, null, "navigation", 2.0,
                async (e, codes) =>
                {
                    await SubmitForAnalysis();
                    Console.WriteLine("Tried Analyze");
                });
            await _editor.AddAction("AnalyzeSimple", "Analyze Code - Tree", new[] { (int)KeyMode.CtrlCmd | (int)KeyCode.KEY_1 },
               null, null, "navigation", 2.5,
               async (e, codes) =>
               {
                   await SubmitForSimpleAnalysis();
                   Console.WriteLine("Tried Analyze");
               });
            await _editor.AddAction("Undo", "Undo", new[] { (int)KeyMode.CtrlCmd | (int)KeyCode.KEY_Z },
                null, null, "navigation", 3.5,
                async (e, codes) =>
                {
                    await Undo();
                    Console.WriteLine("Tried Undo");
                });
            await _editor.AddAction("Redo", "Redo", new[] { (int)KeyMode.CtrlCmd | (int)KeyCode.KEY_Y },
                null, null, "navigation", 4.5,
                async (e, codes) =>
                {
                    await Redo();
                    Console.WriteLine("Tried Redo");
                });
            await _editor.AddAction("ZoomIn", "Zoom in", new[] { (int)KeyMode.CtrlCmd | (int)KeyCode.US_DOT },
                null, null, "navigation", 5.5,
                async (e, codes) =>
                {
                    var editorOptions = new GlobalEditorOptions { FontSize = ++_fontSize };
                    await _editor.UpdateOptions(editorOptions);
                    Console.WriteLine("Tried Redo");
                });
            await _editor.AddAction("ZoomOut", "Zoom out", new[] { (int)KeyMode.CtrlCmd | (int)KeyCode.US_COMMA },
                null, null, "navigation", 6.5,
                async (e, codes) =>
                {
                    var editorOptions = new GlobalEditorOptions { FontSize = --_fontSize };
                    await _editor.UpdateOptions(editorOptions);
                    Console.WriteLine("Tried Redo");
                });
            await _editor.AddAction("Suggest", "Request Suggestion", new[] { (int)KeyMode.CtrlCmd | (int)KeyCode.KEY_J },
                null, null, "navigation", 7.5,
                async (e, codes) =>
                {
                    await Suggest();
                    Console.WriteLine("Tried Redo");
                });

            if (!string.IsNullOrEmpty(AppState.CurrentUser?.UserName))
            {
                await _editor.AddAction("Save", "Save Code", new[] { (int)KeyMode.CtrlCmd | (int)KeyCode.KEY_S }, null,
                    null, "navigation", 8.5,
                    async (_, _) =>
                    {
                        var code = await _editor.GetValue();
                        await OnSave.InvokeAsync(code);
                        Console.WriteLine("Trigger OnSave");
                    });
            }
        }

        private async void EditorDidChangeCursorPosition(CursorPositionChangedEvent eventArgs)
        {
            var model = await _editor.GetModel();
            if (eventArgs == null || model == null) return;
            var word = await model?.GetWordAtPosition(eventArgs.Position);

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
                            GlyphMarginHoverMessage = messages.Select(x => new MarkdownString { Value = x, IsTrusted = true }).ToArray(),
                            ZIndex = hoverContent.ZIndex
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
            AppState.SyntaxTreeInfo = await PublicClient.GetAnalysis(code);
            await SubmitForSimpleAnalysis();
            //AppState.Snippet = code;
        }
        
        public async Task SubmitForSimpleAnalysis()
        {
            var code = await _editor.GetValue();
            //AppState.SyntaxInfoList = LocalAnalyze.AnalyzeSimple(code);
            AppState.SimpleSyntaxTrees = await PublicClient.GetSimpleAnalysis(code);
            var settings = new JsonSerializerSettings() {ReferenceLoopHandling = ReferenceLoopHandling.Ignore, PreserveReferencesHandling = PreserveReferencesHandling.All};
            AppState.TreeContent = JsonConvert.SerializeObject(AppState.SimpleSyntaxTrees, Formatting.Indented, settings);
            Console.WriteLine(AppState.TreeContent[..1000]);
        }
        private async Task Undo()
        {
            await _editor.Trigger("whatever...", "undo", "whatever...");
        }

        private async Task Redo() => await _editor.Trigger("whatever...", "redo", "whatever...");

        private async Task Suggest() => await _editor.Trigger("whatever...", "editor.action.triggerSuggest", "whatever...");

        protected void OnContextMenu(EditorMouseEvent eventArg)
        {
            Console.WriteLine("OnContextMenu : " + JsonSerializer.Serialize(eventArg));
        }
        private async void HandleAppStateStateChange(object _, PropertyChangedEventArgs args)
        {
            //if (args.PropertyName != nameof(AppState.Snippet) && args.PropertyName != nameof(AppState.SyntaxTreeInfo) && args.PropertyName != nameof(AppState.EditorTheme)) return;
            Console.WriteLine($"Editor Handles AppState change of {args.PropertyName} property");
            _shouldRender = true;
            switch (args.PropertyName)
            {
                case nameof(AppState.SyntaxTreeInfo):
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
        }
    }
}
