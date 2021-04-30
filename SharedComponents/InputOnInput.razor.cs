using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace SharedComponents
{
    public partial class InputOnInput
    {
        [Parameter]
        public int Width { get; set; } = 120;
    }
}
