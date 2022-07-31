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
        private readonly CompilerService CompilerService = new();
        private readonly CompileResources CompileResources;
        public ChallengeController(CompileResources compileResources)
        {
            CompileResources = compileResources;
        }

        [HttpPost("submit")]
        public async Task<CodeOutputModel> Submit([FromBody] CodeInputModel codeInput)
        {
            string testCode = codeInput.Solution;
            var outputs = new CodeOutputModel { Outputs = new List<Output>() };
            int index = 1;
            var swTot = new Stopwatch();
            Console.WriteLine("start all tasks");
            swTot.Start();
            
            foreach (var snip in codeInput.Tests)
            {
                var sw = new Stopwatch();

                var output = new Output { TestIndex = index, Test = snip };
                string code = $"{testCode}\n{snip.Append}";
                string expected = snip.TestAgainst;
                Console.WriteLine("Start task 1");
                sw.Start();
                output.Codeout = await CompilerService.SubmitCode(code, CompileResources.PortableExecutableReferences);
                sw.Stop();
                Console.WriteLine($"Complete task 1 in {sw.ElapsedMilliseconds}ms");
                Console.WriteLine("\nStart task 2");
                sw.Restart();
                output.TestResult = await CompilerService.SubmitSolution(code, CompileResources.PortableExecutableReferences, expected);
                output.Test = snip;
                sw.Stop();
                Console.WriteLine($"Complete task 2 in {sw.ElapsedMilliseconds}ms");
                index++;
                outputs.Outputs.Add(output);

            }
            swTot.Stop();
            Console.WriteLine($"Completed all tasks in {swTot.ElapsedMilliseconds}ms");
            return outputs;
        }
    }
}
