using MasterCsharpHosted.Shared;
using MasterCsharpHosted.Shared.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components.Forms;

namespace MasterCsharpHosted.Client.Components
{
    public partial class GenerateCodeForm<TItem> where TItem : ICodeGen
    {
        [Inject]
        private ModalService ModalService { get; set; } = default!;
        [Parameter]
        public EventCallback<string> PromptGenerated { get; set; }
        [Parameter]
        public EventCallback<TItem> CodeGenItemChanged { get; set; }
        [Parameter]
        public TItem CodeGenItem { get; set; }
        private void Submit()
        {
            PromptGenerated.InvokeAsync(CodeGenItem.ToSystemPromptText());
            CodeGenItemChanged.InvokeAsync(CodeGenItem);
        }

        private void AddPropOrParam<T>(T codeGenItem) where T : ICodeGen
        {
            switch (codeGenItem)
            {
                case ICodeGenClass codeGenForm:
                    {
                        codeGenForm.ParamsOrProps.Add(new ParamOrProp(codeGenForm.ParamsOrProps.Count));
                        break;
                    }
                case ICodeGenMethod codeGenMethod:
                    {
                        codeGenMethod.ParamsOrProps.Add(new ParamOrProp(codeGenMethod.ParamsOrProps.Count));
                        break;
                    }
            }

            StateHasChanged();
        }

        private void RemovePropOrParam<T>(T codeGenItem) where T : ICodeGen
        {
            switch (codeGenItem)
            {
                case ICodeGenClass codeGenForm:
                    {
                        if (!codeGenForm.ParamsOrProps.Any()) return;
                        var last = codeGenForm.ParamsOrProps.LastOrDefault();
                        if (last == null) return;
                        codeGenForm.ParamsOrProps.Remove(last);
                        break;
                    }
                case ICodeGenMethod codeGenMethod:
                    {
                        if (!codeGenMethod.ParamsOrProps.Any()) return;
                        var last = codeGenMethod.ParamsOrProps.LastOrDefault();
                        if (last == null) return;
                        codeGenMethod.ParamsOrProps.Remove(last);
                        break;
                    }
            }
            StateHasChanged();
        }

        private void AddMethod()
        {
            if (CodeGenItem is not ICodeGenClass codeGenClass) return;
            codeGenClass.Methods.Add(new CodeGenMethod());
            StateHasChanged();
        }

        private void RemoveMethod()
        {
            if (CodeGenItem is not ICodeGenClass codeGenClass) return;
            if (!codeGenClass.Methods.Any()) return;
            var last = codeGenClass.Methods.LastOrDefault();
            if (last == null) return;
            codeGenClass.Methods.Remove(last);
            StateHasChanged();
        }
    }
}
