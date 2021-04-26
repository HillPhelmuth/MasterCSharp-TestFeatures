using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MasterCsharpHosted.Server.Services;
using MasterCsharpHosted.Shared;

namespace MasterCsharpHosted.Server.Controllers
{
    [Route("api/code")]
    [ApiController]
    public class CodeController : ControllerBase
    {
        private PublicGithubClient _githubClient;
        public CodeController(PublicGithubClient githubClient)
        {
            _githubClient = githubClient;
        }
        private readonly CompilerService _compilerService = new();
        [HttpPost("sugestComplete")]
        public async ValueTask<CustomSuggestionList> GetSuggest([FromBody] SourceInfo sourceInfo)
        {
            var suggestions = await CodeCompletion.GetCodeCompletion(sourceInfo);
            return new CustomSuggestionList {Items = suggestions};
        }

        [HttpPost("compile")]
        public async Task<string> Compile([FromBody] string code)
        {
            var refs = CompileResources.PortableExecutableReferences;
            return await _compilerService.SubmitCode(code, refs);
        }

        [HttpGet("githubCode/{fileName}")]
        public async Task<string> GetFromGithub([FromRoute]string fileName)
        {
            var sw = new Stopwatch();
            sw.Start();
            var code = await _githubClient.CodeFromGithub(fileName);
            sw.Stop();
            Console.WriteLine($"code returned from github: {code}");
            return code;
        }

        [HttpPost("githubCode/{org}/{repo}")]
        public async Task<string> GetFromPublicRepo([FromRoute] string org, [FromRoute] string repo,
            [FromBody] string filePath)
        {
            return await _githubClient.CodeFromPublicRepo(org, repo, filePath);
        }

        [HttpPost("syntax")]
        public Task<SyntaxTreeInfo> GetSyntaxAnalysis([FromBody] string code)
        {
            var analysis = new CodeAnalysis();
            return Task.FromResult(analysis.Analyze(code));
        }
    }
}
