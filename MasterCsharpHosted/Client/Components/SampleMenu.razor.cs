using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MasterCsharpHosted.Shared;
using Microsoft.AspNetCore.Components;

namespace MasterCsharpHosted.Client.Components
{
    public partial class SampleMenu
    {
        [Inject]
        private AppState AppState { get; set; }
        [Parameter]
        public EventCallback<bool> OnCloseMenu { get; set; }

        private void MouseOverContent(string content)
        {
            AppState.Content = content;
        }
    }
}
