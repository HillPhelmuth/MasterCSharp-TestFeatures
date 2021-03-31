using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MasterCsharpHosted.Shared;

namespace MasterCsharpHosted.Client
{
    public class PublicClient : IPublicClient
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
            {
                sw.Stop();
                Console.WriteLine($"api/code/compile returned an error in {sw.ElapsedMilliseconds}ms");
                return $"Error on compile. {apiResult.ReasonPhrase}";
            }
            var result = await apiResult.Content.ReadAsStringAsync();
            sw.Stop();
            Console.WriteLine($"api/code/compile returned an value in {sw.ElapsedMilliseconds}ms");
            return result;
        }

        public async Task<string> GetFromGithubRepo(string fileName)
        {
            var sw = new Stopwatch();
            sw.Start();
            string apiResult = await Client.GetStringAsync($"api/code/githubCode/{fileName}");
            sw.Stop();
            Console.WriteLine($"api/githubCode returned a value in {sw.ElapsedMilliseconds}ms\r\nResult: {apiResult}");
            return apiResult;
        }
    }
}
