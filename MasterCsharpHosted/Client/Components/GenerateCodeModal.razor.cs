using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using MasterCsharpHosted.Shared;
using MasterCsharpHosted.Shared.Services;

namespace MasterCsharpHosted.Client.Components
{
    public partial class GenerateCodeModal
    {
        private List<CodeGenType> CodeGenTypes => Enum.GetValues(typeof(CodeGenType)).Cast<CodeGenType>().ToList();
        private class SelectTypeForm
        {
            public CodeGenType CodeGenType { get; set; }
        }
        private SelectTypeForm _selectTypeForm = new(); 
        [Inject]
        private ModalService ModalService { get; set; } = default!;

        private bool isStarted;
        private void HandleGenerateSystemPrompt(string prompt)
        {
            var modalParams = new ModalParameters
            {
                { "GeneratedPrompt", prompt }
            };
            ModalService.Close(new ModalResults(true, modalParams));
        }

        private void Submit()
        {
            isStarted = true;
        }
    }
}
