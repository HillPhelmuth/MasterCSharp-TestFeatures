using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Configuration;
using MasterCsharpHosted.Client;
using MasterCsharpHosted.Client.Shared;
using MasterCsharpHosted.Shared;
using MasterCsharpHosted.Client.Components;
using SharedComponents;
using BlazorMonaco;

namespace MasterCsharpHosted.Client.Shared
{
    public partial class Spin
    {
        [Parameter]
        public bool IsWorking { get; set; }
        [Parameter]
        public SpinStyle SpinStyle { get; set; }
        [Parameter]
        public string Content { get; set; }
        private string Css => Enum.GetName(SpinStyle)?.ToLower() ?? "";
    }
    public enum SpinStyle
    {
        Circle, Lines, Bubble
    }
}