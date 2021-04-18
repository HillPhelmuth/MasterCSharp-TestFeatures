using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MasterCsharpHosted.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace MasterCsharpHosted.Client.Shared
{
    public partial class InitAppState
    {
        [Inject]
        private AppState AppState { get; set; }
        [Inject]
        private AuthenticationStateProvider AuthState { get; set; }
        [Inject]
        private PublicClient PublicClient { get; set; }


        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                var userData = await AuthState.GetAuthenticationStateAsync();
                string userName = userData?.User?.Identity?.Name;
                if (userData?.User?.Identity?.IsAuthenticated ?? false)
                {
                    var user = await PublicClient.GetOrAddUser(userName);
                    AppState.CurrentUser = user;
                }
            }
            await base.OnAfterRenderAsync(firstRender);
        }

        protected override bool ShouldRender() => false;
    }
}
