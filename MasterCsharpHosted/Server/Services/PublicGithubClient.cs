using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace MasterCsharpHosted.Server.Services
{
    public class PublicGithubClient
    {
        public HttpClient Client { get; }
        private readonly string baseUrl = @"https://api.github.com/repos";

        private readonly string reposUrl = "repos/HillPhelmuth/NakedCodeSnippets/contents/NakedCodeSnippets";
        public PublicGithubClient(HttpClient client)
        {
            client.BaseAddress = new Uri("https://api.github.com/");
            // GitHub API versioning
            client.DefaultRequestHeaders.Add("Accept",
                "application/vnd.github.v3.raw");
            // GitHub requires a user-agent
            client.DefaultRequestHeaders.Add("User-Agent",
                "MasterCSharp-request");

            Client = client;
            
           // client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3.raw");
        }
        public async Task<string> CodeFromGithub(string filename)
        {
            var sw = new Stopwatch();
            sw.Start();
            var code = await Client.GetStringAsync($"{reposUrl}/{filename}.cs");
            sw.Stop();
            Console.WriteLine($"Retrieved code from Github in {sw.ElapsedMilliseconds}ms");
            return code;
        }

        public async Task<string> CodeFromPublicRepo(string githubName, string repoName, string filepath)
        {
            if (!filepath.Contains("."))
                return "Nope!, provide a file extension. I suggest '.cs'";
            var sw = new Stopwatch();
            sw.Start();
            var code = await Client.GetStringAsync($"repos/{githubName}/{repoName}/contents/{filepath}");
            sw.Stop();
            Console.WriteLine($"Retrieved code from Github in {sw.ElapsedMilliseconds}ms");
            return code;
        }
    }
}
