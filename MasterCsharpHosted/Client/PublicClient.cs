using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace MasterCsharpHosted.Client
{
    public class PublicClient
    {
        public HttpClient Client { get; }
        public PublicClient(HttpClient httpClient)
        {
            Client = httpClient;
        }

        public async Task<string> CompileCodeAsync(string code)
        {
            var sw = new Stopwatch();
            sw.Start();
            var apiResult = await Client.PostAsJsonAsync("api/code/compile", code);
            if (!apiResult.IsSuccessStatusCode)
                return $"Error on compile. {apiResult.ReasonPhrase}";
            return await apiResult.Content.ReadAsStringAsync();
        }
    }
}
