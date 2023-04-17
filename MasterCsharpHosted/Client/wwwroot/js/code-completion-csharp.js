//var require = { paths: { vs: '_content/BlazorMonaco/lib/monaco-editor/min/vs' } };

require(["vs/editor/editor.main"], function () {
    monaco.languages.registerCompletionItemProvider("csharp", getcsharpCompletionProvider(monaco));
    monaco.languages.registerSignatureHelpProvider("csharp", getcsharpSignatureHelpProvidor(monaco));
});

function getcsharpSignatureHelpProvidor(monaco) {
    return {
        signatureHelpTriggerCharacters: ["("],
        provideSignatureHelp: function(model, position) {
            var request = createRequestObject(model, position);
            return new Promise((resolve, reject) => {
                $.ajax({
                    url: "/api/code/suggestSignature",
                    data: JSON.stringify(request),
                    type: "Post",
                    traditional: true,
                    contentType: "application/json",
                    success: function (data) {
                        if (data) {
                            const returnValue = {
                                signatures: [],
                                activeSignature: data.activeSignature,
                                activeParameter: data.activeParameter
                            };
                            for (let i = 0; i < data.signatures.length; i++) {
                                const signatureInfo = {
                                    label: data.signatures[i].label,
                                    documentation: data.signatures[i].structuredDocumentation.summaryText,
                                    parameters: []
                                };
                                
                                for (let j = 0; j < data.signatures[i].parameters; j++) {
                                    const parameterInfo = {
                                        label: data.signatures[i].parameters[j].label,
                                        documentation: this.getParameterDocumentation(data.signatures[i].parameters[j])
                                    };
                                    signatureInfo.parameters.push(parameterInfo);
                                }
                                returnValue.signatures.push(signatureInfo);
                            }
                            const returnObj = {
                                value: returnValue,
                                dispose: () => { }
                            };
                            resolve(returnObj);
                        }
                    },
                    error: function (error) {
                        console.error(error);
                    }

                });
            });
        }
}

}
function getcsharpCompletionProvider(monaco) {
    return {
        triggerCharacters: ['.','='],
        provideCompletionItems: function (model, position) {

            const textUntilPosition = model.getValueInRange({ startLineNumber: 1, startColumn: 1, endLineNumber: position.lineNumber, endColumn: position.column });
            const cursor = textUntilPosition.length;
            var sourceInfo = { SourceCode: model.getValue(), lineNumberOffsetFromTemplate: cursor };

            //var funcUrl = "https://codecompletionfunction.azurewebsites.net/api/CompleteCode";
            return new Promise((resolve, reject) => {
                $.ajax({
                    url: "/api/code/sugestComplete",
                    data: JSON.stringify(sourceInfo),
                    type: "POST",
                    traditional: true,
                    contentType: "application/json",
                    success: function (data) {
                        if (data && data.items) {
                            const availableResolvers = [];
                            for (let i = 0; i < data.items.length; i++) {
                                const withSymbol = {
                                    label: data.items[i].label,
                                    insertText: data.items[i].insertText,
                                    kind: convertRoslynKindToMonacoKind(data.items[i].kind),
                                    detail: data.items[i].detail,
                                    documentation: data.items[i].documentation
                                };
                                //console.log('with symbolkind: ' + data.result.items[i].properties.SymbolKind);
                                availableResolvers.push(withSymbol);

                            }
                            console.log("Completions from function: " + availableResolvers.length);
                            const returnObj = {
                                suggestions: availableResolvers
                            };
                            resolve(returnObj);
                        }
                    },
                    error: function (error) {
                        console.log(error);
                    }
                });
            });

        }
    };
}
function convertRoslynKindToMonacoKind(kind) {
    switch (kind) {
        case "9": return monaco.languages.CompletionItemKind.Method;
        case "15": return monaco.languages.CompletionItemKind.Property;
        case "11": return monaco.languages.CompletionItemKind.Class;
        case "6": return monaco.languages.CompletionItemKind.Field;
        case "8": return monaco.languages.CompletionItemKind.Keyword;
        case "5": return monaco.languages.CompletionItemKind.Event;
        case "17": return monaco.languages.CompletionItemKind.TypeParameter;
        
    }
    return monaco.languages.CompletionItemKind.Variable;
}
function createRequestObject(model, position) {

    var line = position.lineNumber - 1;
    var col = position.column - 1;

    const request = {
        SourceCode: model.getValue(),
        Line: line,
        Column: col
    };

    return request;
}
function getParameterDocumentation(parameter) {
    const summary = parameter.documentation;
    if (summary.length > 0) {
        const paramText = `**${parameter.name}**: ${summary}`;
        return {
            value: paramText
        };
    }

    return "";
}
