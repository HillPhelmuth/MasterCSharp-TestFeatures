if (!require.getConfig().paths.vs)
    require.config({ paths: { 'vs': '_content/BlazorMonaco/lib/monaco-editor/min/vs' } });

require(["vs/editor/editor.main"], function () {
    monaco.languages.registerCompletionItemProvider('csharp', getcsharpCompletionProvider(monaco));
});
function getcsharpCompletionProvider(monaco) {
    return {
        triggerCharacters: ['.', '(', ',', ')'],
        provideCompletionItems: function (model, position) {

            var textUntilPosition = model.getValueInRange({ startLineNumber: 1, startColumn: 1, endLineNumber: position.lineNumber, endColumn: position.column });
            var cursor = textUntilPosition.length;
            var sourceInfo = { SourceCode: model.getValue(), lineNumberOffsetFromTemplate: cursor };
            //var funcUrl = "https://codecompletionfunction.azurewebsites.net/api/CompleteCode";
            return new Promise((resolve, reject) => {
                $.ajax({
                    url: '/api/code/sugestComplete',
                    data: JSON.stringify(sourceInfo),
                    type: 'POST',
                    traditional: true,
                    contentType: 'application/json',
                    success: function (data) {
                        var availableResolvers = [];
                        if (data && data.items) {
                            for (var i = 0; i < data.items.length; i++) {
                                var withSymbol = {
                                    label: data.items[i].label,
                                    insertText: data.items[i].insertText,
                                    kind: convertSymbolKindToMonacoEnum(data.items[i].kind),
                                    detail: data.items[i].detail,
                                    documentation: data.items[i].documentation
                                };
                                //console.log('with symbolkind: ' + data.result.items[i].properties.SymbolKind);
                                availableResolvers.push(withSymbol);

                            }
                            console.log("Completions from function: " + availableResolvers.length);
                            var returnObj = {
                                suggestions: availableResolvers
                            };
                            resolve(returnObj);
                        }
                    },
                    error: function (error) {
                        console.log(error);
                    },
                });
            });

        }
    };
}
function convertSymbolKindToMonacoEnum(kind) {
    switch (kind) {
        case "9": return monaco.languages.CompletionItemKind.Method;
        case "15": return monaco.languages.CompletionItemKind.Property;
        case "11": return monaco.languages.CompletionItemKind.Class;
        case "6": return monaco.languages.CompletionItemKind.Field;
        case "8": return monaco.languages.CompletionItemKind.Variable;
    }
}