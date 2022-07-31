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
        private readonly CompileResources _compileResources;

        public ServerApi(PublicGithubClient githubClient, CompileResources compileResources)
        {
            _githubClient = githubClient;
            _compileResources = compileResources;
        }

        public async Task<string> CompileCodeAsync(string code)
        {
            var refs = _compileResources.PortableExecutableReferences;
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
