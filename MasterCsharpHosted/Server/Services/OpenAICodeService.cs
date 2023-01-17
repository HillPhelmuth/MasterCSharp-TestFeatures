using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenAI.GPT3.Interfaces;
using OpenAI.GPT3.ObjectModels;
using OpenAI.GPT3.ObjectModels.RequestModels;
using OpenAI.GPT3.ObjectModels.ResponseModels;

namespace MasterCsharpHosted.Server.Services
{
    public class OpenAICodeService
    {
        private IOpenAIService _openAiService;

        public OpenAICodeService(IOpenAIService openAiService)
        {
            _openAiService = openAiService;
        }

        public async Task<string> ExplainAsync(string code, string userName)
        {
            var promptBuilder = new StringBuilder();
            //promptBuilder.AppendLine("```");
            promptBuilder.AppendLine("//c# code");
            promptBuilder.AppendLine(code);
            promptBuilder.AppendLine("//Describe the c# code above in human readable format");
            promptBuilder.Append("//");
            //promptBuilder.AppendLine("```");
            try
            {
                //ConsoleExtensions.WriteLine("Completion Test:", ConsoleColor.DarkCyan);
                var prompt = promptBuilder.ToString().Replace("\r\n", "\n");

                var completionCreateRequest = new CompletionCreateRequest()
                {
                    Prompt = prompt,
                    //    PromptAsList = new []{"Once upon a time"},
                    MaxTokens = 256,
                    Temperature = 0,
                    N = 1,
                    BestOf = 1,
                    Echo = false,
                    StopAsList = new List<string> { "//c# code", "//Explain " },
                    TopP = 1,
                    FrequencyPenalty = 0.5f,
                    //LogProbs = 0,
                    PresencePenalty = 0,
                    User = userName
                };
                Console.WriteLine($"Request Payload:\n{JsonConvert.SerializeObject(completionCreateRequest, Formatting.Indented)}");
                CompletionCreateResponse completionResult = await _openAiService.Completions.CreateCompletion(completionCreateRequest, Models.CodeDavinciV2);

                if (completionResult.Successful)
                {
                    Console.WriteLine($"Code Explanation choices:\n{JsonConvert.SerializeObject(completionResult.Choices, Formatting.Indented)}");
                }
                else
                {
                    if (completionResult.Error == null)
                    {
                        throw new Exception("Unknown Error");
                    }

                    Console.WriteLine($"{completionResult.Error.Code}: {completionResult.Error.Message}");
                }

                var best = completionResult.Choices.FirstOrDefault();
                Console.WriteLine(best);
                //var conversationResponse = new ConvoBit($"You: {text}", $"Marv: {best.Text}");
                //convoBits.Add(conversationResponse);
                return best.Text;

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
