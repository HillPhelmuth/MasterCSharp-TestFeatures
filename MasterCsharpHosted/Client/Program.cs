using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MasterCsharpHosted.Shared;
using MasterCsharpHosted.Shared.Services;

namespace MasterCsharpHosted.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            //builder.RootComponents.Add<App>("#app");

            builder.Services.AddTransient(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddOidcAuthentication(options =>
            {
                builder.Configuration.Bind("Auth0", options.ProviderOptions);
                options.ProviderOptions.ResponseType = "code";
            });
            builder.Services.AddHttpClient<PublicClient>(c => c.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
            builder.Services.AddScoped<IPublicClient, PublicClient>();
            builder.Services.AddScoped<ICodeClient, PublicClient>();
            builder.Services.AddScoped<IUserClient, PublicClient>();
            builder.Services.AddScoped<IChallengeClient, PublicClient>();
            builder.Services.AddScoped<ModalService>();
            builder.Services.AddSingleton<AppState>();
            await builder.Build().RunAsync();
        }
    }
}
