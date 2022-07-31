using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MasterCsharpHosted.Shared;
using Microsoft.AspNetCore.Components;
using SharedComponents;

namespace ChallengeModule
{
    public partial class ResultTable : AppComponentBase
    {
        protected override List<string> InterestingProperties => new()
        {
            nameof(AppState.ActiveChallenge),
            nameof(AppState.ChallengeOutput)
        };
        protected override void UpdateState(object sender, PropertyChangedEventArgs e)
        {
            if (!InterestingProperties.Contains(e.PropertyName)) return;
            if (e.PropertyName == nameof(AppState.ActiveChallenge))
            {
                AppState.ChallengeOutput = null;
            }
            StateHasChanged();
        }
    }
}
