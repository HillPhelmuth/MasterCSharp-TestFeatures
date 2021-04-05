using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MasterCsharpHosted.Shared;
using Microsoft.AspNetCore.Components;

namespace ChallengeModule
{
    public partial class ResultTable<TItem>
    {
        [Parameter]
        public RenderFragment TableHeader { get; set; }

        [Parameter]
        public RenderFragment<TItem> RowTemplate { get; set; }

        [Parameter]
        public IReadOnlyList<TItem> Items { get; set; }
        [Parameter]
        public Func<TItem, TestResult> CssSelector { get; set; }

    }
}
