using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace CodeSyntaxModule
{
    public partial class SyntaxGroupWidget : IDisposable
    {
        [Parameter]
        public SyntaxGroup Group { get; set; }

        private string GroupCss => GetAllCss();

        private string GetAllCss()
        {
            string baseCls = "custom-group";
            if (Group.Selected) baseCls += " selected";
            baseCls += $" {Group.Kind.ToString().ToLower()}";
            return baseCls += $" {Group.Kind.ToString().ToLower()}";
        }
        protected override Task OnInitializedAsync()
        {
            Group.Changed += StateHasChanged;
            return base.OnInitializedAsync();
        }

        public void Dispose()
        {
            Group.Changed -= StateHasChanged;
        }
    }
}
