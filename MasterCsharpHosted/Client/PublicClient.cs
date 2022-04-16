using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MasterCsharpHosted.Shared;

namespace MasterCsharpHosted.Client
{
    public class PublicClient : ICodeClient, IUserClient, IChallengeClient
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
            string result = await apiResult.Content.ReadAsStringAsync();
            sw.Stop();
            Console.WriteLine($"api/code/compile returned an value in {sw.ElapsedMilliseconds}ms");
            return result;
        }
        public async Task<CodeOutputModel> SubmitChallenge(ChallengeModel challenge)
        {
            var sw = new Stopwatch();
            sw.Start();
            var apiResult = await Client.PostAsJsonAsync($"api/challenge/submit", challenge);
            string result = await apiResult.Content.ReadAsStringAsync();
            sw.Stop();
            Console.WriteLine($"challenge submit too {sw.ElapsedMilliseconds}ms");
            var output = JsonSerializer.Deserialize<CodeOutputModel>(result);
            return output;
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

        public async Task<string> GetFromPublicRepo(string org, string repo, string filePath)
        {
            var sw = new Stopwatch();
            sw.Start();
            var apiResult = await Client.PostAsJsonAsync($"api/code/githubCode/{org}/{repo}",filePath);
            if (!apiResult.IsSuccessStatusCode)
                return $"Failed to Retrieve code:\r\n{apiResult.ReasonPhrase}";

            string result = await apiResult.Content.ReadAsStringAsync();
            sw.Stop();
            Console.WriteLine($"api/githubCode returned a value in {sw.ElapsedMilliseconds}ms\r\nResult: {result}");
            return result;
        }

        public async Task<AppUser> GetOrAddUser(string userName)
        {
            var sw = new Stopwatch();
            sw.Start();
            var apiResult = await Client.GetFromJsonAsync<AppUser>($"api/appUser/getUser/{userName}");
            if (apiResult != null && apiResult.UserName != "NONE")
            {
                sw.Stop();
                Console.WriteLine($"api/appUser/getUser returned a value in {sw.ElapsedMilliseconds}ms\r\nResult: {apiResult}");
                return apiResult;
            }

            var addResult = await Client.PostAsJsonAsync("api/appUser/addUser", new AppUser {UserName = userName, Snippets = new List<UserSnippet>{new("Default", CodeSnippets.DefaultCode, "Default sample snippet")}, CompletedChallenges = new List<CompletedChallenge>(), IsAuthenticated = true});
            string result = await addResult.Content.ReadAsStringAsync();
            sw.Stop();
            Console.WriteLine($"api/appUser/addUser returned a value in {sw.ElapsedMilliseconds}ms\r\nResult: {result}");
            return JsonSerializer.Deserialize<AppUser>(result);
        }

        public async Task<SyntaxTreeInfo> GetAnalysis(string code)
        {
            var sw = new Stopwatch();
            sw.Start();
            var apiResult = await Client.PostAsJsonAsync("api/code/syntax", code);
            if (!apiResult.IsSuccessStatusCode)
            {
                sw.Stop();
                Console.WriteLine($"api/code/syntax returned an error in {sw.ElapsedMilliseconds}ms");
                Console.WriteLine($"Error on Analyze. {apiResult.ReasonPhrase}");
                return null;
            }
            string result = await apiResult.Content.ReadAsStringAsync();
            sw.Stop();
            Console.WriteLine($"api/code/syntax returned value in {sw.ElapsedMilliseconds}ms");
            var syntaxResult = Newtonsoft.Json.JsonConvert.DeserializeObject<SyntaxTreeInfo>(result);
            Console.WriteLine($"Namespaces:\r\n{string.Join("\r\n\t", syntaxResult.NameSpaces?.Select(x => x.Name))}");
            Console.WriteLine($"Classes:\r\n{string.Join("\r\n\t", syntaxResult.NameSpaces.SelectMany(x => x?.Classes?.Select(c => x.Name)))}");
            return syntaxResult;
        }
        public async Task<bool> UpdateUser(AppUser user)
        {
            var apiResult = await Client.PostAsJsonAsync("api/appUser/updateUser", user);
            return apiResult.IsSuccessStatusCode;
        }
    }
}
