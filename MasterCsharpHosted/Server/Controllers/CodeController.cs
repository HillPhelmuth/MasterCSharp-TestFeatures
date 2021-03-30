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
        public dynamic GetSuggest([FromBody] SourceInfo sourceInfo)
        {
            var suggestions = CodeCompletion.GetCodeCompletion(sourceInfo);
            return suggestions;
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

    }
}
