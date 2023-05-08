using System.Collections.Generic;
using System.Threading.Tasks;
using MasterCsharpHosted.Server.Services;
using MasterCsharpHosted.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MasterCsharpHosted.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OpenAiController : ControllerBase
    {
        private readonly OpenAICodeService _openAiCodeService;
        public OpenAiController(OpenAICodeService openAiCodeService)
        {
            _openAiCodeService = openAiCodeService;
        }
        [HttpPost("explain/{userName}")]
        public async ValueTask<string> ExplainCode([FromBody] string code, [FromRoute] string userName)
        {
            return await _openAiCodeService.ExplainCodeAsync(code, userName);
        }
        [HttpPost("document/{userName}")]
        public async ValueTask<string> DocumentCode([FromBody] string code, [FromRoute] string userName)
        {
            return await _openAiCodeService.GenerateMarkdownDocs(code, userName);
        }

        [HttpPost("generateCode/{userName}")]
        public async ValueTask<string> GenerateCode([FromBody] CodeGenForm codeGenForm, [FromRoute] string userName)
        {
            return await _openAiCodeService.GenerateCodeAsync(codeGenForm, userName);
        }
    }
}
