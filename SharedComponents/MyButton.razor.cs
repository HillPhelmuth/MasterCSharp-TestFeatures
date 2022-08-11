using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace SharedComponents
{
    public partial class MyButton : AppComponentBase
    {
        [Parameter]
        public EventCallback<MouseEventArgs> OnClick { get; set; }
        [Parameter]
        public string? Label { get; set; }

        [Parameter] 
        public string BackgroundColor { get; set; } = "#9400d3";
        [Parameter] 
        public string TextColor { get; set; } = "#ffffff";

        [Parameter] 
        public string FontSize { get; set; } = "1rem";

        private string _style = "";
        protected override Task OnParametersSetAsync()
        {
            _style = $"background-color: {BackgroundColor}; color: {TextColor}; font-size: {FontSize}";
            return base.OnParametersSetAsync();
        }

        private void Click(MouseEventArgs e)
        {
            OnClick.InvokeAsync(e);
        }
    }
    

}
