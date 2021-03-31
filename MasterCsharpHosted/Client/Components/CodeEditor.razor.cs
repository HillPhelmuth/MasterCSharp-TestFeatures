using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using BlazorMonaco;
using MasterCsharpHosted.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace MasterCsharpHosted.Client.Components
{
    public partial class CodeEditor : IDisposable
    {
        [Inject]
        private IPublicClient PublicClient { get; set; }
        [Inject]
        private AppState AppState { get; set; }
        [Inject]
        private IJSRuntime Js { get; set; }

        [Parameter]
        public string CodeSnippet { get; set; }
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
                Theme = "vs-dark",
                GlyphMargin = true,
                Value = CodeSnippets.DefaultCode
            };
        }

        protected async Task EditorOnDidInit(MonacoEditorBase editorBase)
        {
            await _editor.AddCommand((int)KeyMode.CtrlCmd | (int)KeyCode.KEY_H, (editor, keyCode) =>
            {
                Console.WriteLine("Ctrl+H : Initial editor command is triggered.");
            });
            await _editor.AddAction("executeAction", "Execute Code",
                new[] { (int)KeyMode.CtrlCmd | (int)KeyCode.Enter }, null, null, "navigation", 1.5,
                async (ed, keyCodes) =>
                {
                    await SubmitCode();
                    Console.WriteLine("Code Executed from Editor Command");
                });
            await _editor.AddAction("Undo", "Undo", new[] {(int) KeyMode.CtrlCmd | (int) KeyCode.KEY_Z},
                null, null, "navigation", 2.5,
                async (e, codes) =>
                {
                    await Undo();
                    Console.WriteLine("Tried Undo");
                });
            await _editor.AddAction("Redo", "Redo", new[] { (int)KeyMode.CtrlCmd | (int)KeyCode.KEY_Y },
                null, null, "navigation", 3.5,
                async (e, codes) =>
                {
                    await Redo();
                    Console.WriteLine("Tried Redo");
                });
            await _editor.AddAction("ZoomIn", "Zoom in", new[] { (int)KeyMode.CtrlCmd | (int)KeyCode.US_DOT },
                null, null, "navigation", 4.5,
                async (e, codes) =>
                {
                    var editorOptions = new GlobalEditorOptions { FontSize = ++_fontSize };
                    await _editor.UpdateOptions(editorOptions);
                    Console.WriteLine("Tried Redo");
                });
            await _editor.AddAction("ZoomOut", "Zoom out", new[] { (int)KeyMode.CtrlCmd | (int)KeyCode.US_COMMA },
                null, null, "navigation", 5.5,
                async (e, codes) =>
                {
                    var editorOptions = new GlobalEditorOptions { FontSize = --_fontSize };
                    await _editor.UpdateOptions(editorOptions);
                    Console.WriteLine("Tried Redo");
                });
        }

        private async void EditorDidChangeCursorPosition(CursorPositionChangedEvent eventArgs)
        {
            var model = await _editor.GetModel();
            if (eventArgs == null || model == null) return;
            var word = await model?.GetWordAtPosition(eventArgs.Position);

            var pos = eventArgs.Position;
            int lineEndCol = await model.GetLineLastNonWhitespaceColumn(pos.LineNumber);
            string lineContent = await model.GetLineContent(pos.LineNumber);
            Console.WriteLine($"Word at current cursor: {word?.Word}");
            string[] deltas = _deltaDecorationIds;
            var listDelta = new List<ModelDeltaDecoration>();
            
            foreach (var hoverContent in AppHoverContent.AllAppHoverItems)
            {
                foreach ((string key, var messages) in hoverContent.KeyWordMessages.Where(x => lineContent.Contains(x.Key)))
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

        private async Task SubmitCode()
        {
            string code = await _editor.GetValue();
            string returnedValue = await PublicClient.CompileCodeAsync(code);
            AppState.AddLineToOutput(returnedValue);
        }

        private async Task Undo() => await Js.InvokeVoidAsync("blazorMonaco.editor.trigger", _editor.Id, "whatever...", "undo", "whatever...");

        private async Task Redo() => await Js.InvokeVoidAsync("blazorMonaco.editor.trigger", _editor.Id, "whatever...", "redo", "whatever...");

        protected void OnContextMenu(EditorMouseEvent eventArg)
        {
            Console.WriteLine("OnContextMenu : " + JsonSerializer.Serialize(eventArg));
        }
        private async void HandleAppStateStateChange(object _, PropertyChangedEventArgs args)
        {
            if (args.PropertyName != nameof(AppState.Snippet)) return;
            _shouldRender = true;
            await _editor.SetValue(AppState.Snippet);
            StateHasChanged();
        }

        public void Dispose()
        {
            AppState.PropertyChanged -= HandleAppStateStateChange;
        }
    }
}
