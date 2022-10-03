using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MasterCsharpHosted.Server.Services;
using MasterCsharpHosted.Shared;
using Newtonsoft.Json;
using OmniSharp.Models.SignatureHelp;

namespace MasterCsharpHosted.Server.Controllers
{
    [Route("api/code")]
    [ApiController]
    public class CodeController : ControllerBase
    {
        private readonly PublicGithubClient _githubClient;
        private readonly CompileResources _compileResources;
        private readonly CodeCompletion _codeCompletion;
        private readonly CodeAnalysis _codeAnalysis;
        public CodeController(PublicGithubClient githubClient, CompileResources compileResources, CodeCompletion codeCompletion, CodeAnalysis codeAnalysis)
        {
            _githubClient = githubClient;
            _compileResources = compileResources;
            _codeCompletion = codeCompletion;
            _codeAnalysis = codeAnalysis;
        }
        private readonly CompilerService _compilerService = new();
        [HttpPost("sugestComplete")]
        public async ValueTask<CustomSuggestionList> GetSuggest([FromBody] SourceInfo sourceInfo)
        {
            var refs = _compileResources.PortableExecutableReferences;
            var suggestions = await _codeCompletion.GetCodeCompletion(sourceInfo, refs);
            return new CustomSuggestionList {Items = suggestions};
        }

        [HttpPost("suggestSignature")]
        public async ValueTask<SignatureHelpResponse> GetSignatureHelp([FromBody] SourceInfo request)
        {
            var refs = _compileResources.PortableExecutableReferences;
            return await _codeCompletion.GetMethodSignatureItems(request, refs);
        }
        [HttpPost("compile")]
        public async Task<string> Compile([FromBody] string code)
        {
            var refs = _compileResources.PortableExecutableReferences;
            return await _compilerService.SubmitCode(code, refs);
        }
        
        [HttpPost("decompile")]
        public async Task<string> Decompile([FromBody] string code)
        {
            var refs = _compileResources.PortableExecutableReferences;
            return await DecompileService.CompileAndDecompileCode(code, refs);
        }

        [HttpPost("decompileFile")]
        public async Task<string> DecompileFile([FromBody] byte[] fileContents)
        {
            Stream content = new MemoryStream(fileContents);
            return await DecompileService.DecompileAssemblyStreamAsync(content);
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
            return Task.FromResult(_codeAnalysis.Analyze(code));
        }
        [HttpPost("simpleSyntax")]
        public Task<string> GetSimpleSyntax([FromBody] string code)
        {
            var analysis = _codeAnalysis.AnalyzeSimpleTree(code);
            return Task.FromResult(JsonConvert.SerializeObject(analysis, settings:new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore} ));
        }
    }
}
