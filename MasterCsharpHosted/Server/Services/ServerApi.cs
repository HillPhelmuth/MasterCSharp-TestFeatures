using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MasterCsharpHosted.Shared;

namespace MasterCsharpHosted.Server.Services
{
    public class ServerApi : IPublicClient
    {
        private readonly CompilerService _compilerService = new();
        private readonly PublicGithubClient _githubClient;

        public ServerApi(PublicGithubClient githubClient)
        {
            _githubClient = githubClient;
        }

        public async Task<string> CompileCodeAsync(string code)
        {
            var refs = CompileResources.PortableExecutableReferences;
            return await _compilerService.SubmitCode(code, refs);
        }

        public async Task<string> GetFromGithubRepo(string fileName)
        {
            var sw = new Stopwatch();
            sw.Start();
            var code = await _githubClient.CodeFromGithub(fileName);
            sw.Stop();
            Console.WriteLine($"code returned from github: {code}");
            return code;
        }
    }
}
