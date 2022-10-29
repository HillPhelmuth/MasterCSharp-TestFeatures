using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MasterCsharpHosted.Server.Services;
using MasterCsharpHosted.Shared;

namespace MasterCsharpHosted.Server.Controllers
{
    [Route("api/challenge")]
    [ApiController]
    public class ChallengeController : ControllerBase
    {
        private readonly CompilerService _compilerService = new();
        private readonly CompileResources _compileResources;
        public ChallengeController(CompileResources compileResources)
        {
            _compileResources = compileResources;
        }

        [HttpPost("submit")]
        public async Task<CodeOutputModel> Submit([FromBody] CodeInputModel codeInput)
        {
            var swTot = new Stopwatch();
            Console.WriteLine("start all tasks");
            swTot.Start();
            var outputs = await _compilerService.SubmitChallenge(codeInput, _compileResources.PortableExecutableReferences);

            swTot.Stop();
            Console.WriteLine($"Completed all tasks in {swTot.ElapsedMilliseconds}ms");
            return outputs;
        }
    }
}
