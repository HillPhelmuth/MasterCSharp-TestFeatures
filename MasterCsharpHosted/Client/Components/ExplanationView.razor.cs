using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace MasterCsharpHosted.Client.Components
{
    public partial class ExplanationView
    {
        [Parameter]
        public string Content { get; set; }

        private List<string> _contentItems = new();

        protected override Task OnParametersSetAsync()
        {
            if (string.IsNullOrEmpty(Content) || !Content.Contains("//")) return base.OnParametersSetAsync();
            var items = Content.Split("//");
            _contentItems = items.ToList();
            return base.OnParametersSetAsync();
        }
    }
}
