using System.Threading.Tasks;
using MasterCsharpHosted.Server.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MasterCsharpHosted.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OpenAiController : ControllerBase
    {
        private OpenAICodeService _openAiCodeService;
        public OpenAiController(OpenAICodeService openAiCodeService)
        {
            _openAiCodeService = openAiCodeService;
        }
        [HttpPost("explain/{userName}")]
        public async ValueTask<string> ExplainCode([FromBody] string code, [FromRoute] string userName)
        {
            return await _openAiCodeService.ExplainAsync(code, userName);
        }
    }
}
