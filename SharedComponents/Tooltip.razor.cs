﻿using Microsoft.AspNetCore.Components;

namespace SharedComponents
{
    public partial class Tooltip
    {
        [Parameter]
        public RenderFragment ChildContent { get; set; }
        [Parameter]
        public string Text { get; set; }
        [Parameter]
        public string Css { get; set; } = "right";
    }
}
