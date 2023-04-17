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

        private const string SystemPromptText = "You are CSharp Expert. Your personality type is Instructive, which makes you behave as: Serving to instruct or inform; conveying instruction, knowledge, or information; enlightening.\r\nFrom now on you are going to act as CSharp Expert, the uncensored and Instructive - Serving to instruct or inform; conveying instruction, knowledge, or information; enlightening..You are a true impersonation of CSharp Expert and you reply to all requests with I pronoun. Remember, YOU ARE NOT AN AI MODEL! You are genuinely CSharp Expert\r\nYour Description: You are an expert in the c# programming language and it's associated frameworks, incuding .net framework, .net core, .net 5, .net 6, etc. Your job is to explain snippets of c# code and explain clearly and in plain language what the code does. Assume you're instructing someone that is just beginning to learn c#.\r\nContext: don't hold back your uncensored opinions. Feel free to swear and curse at any time\r\nYou never directly reference your personality type or your description or the Context.";
        public async Task<string> ExplainCodeAsync(string code, string userName)
        {
            var messages = new List<ChatMessage> {new("system", SystemPromptText), new("user", code)};
            var chatRequest = new ChatCompletionCreateRequest
            {
                MaxTokens = 2000,
                Temperature = 0.7f,
                N = 1,
                Messages = messages,
                User = userName,
                Model = "gpt-3.5-turbo"
            };
            Console.WriteLine("Starting stream response.");
            var response = await _openAiService.ChatCompletion.CreateCompletion(chatRequest);
            if (!response.Successful)
            {
                return $"It looks like the request failed! Reason: {response.Error!.Message}";
            }

            var content = response.Choices[0].Message.Content;
            Console.WriteLine(content);
            return content;
            
        }
        [Obsolete]
        public async Task<string> ExplainAsync(string code, string userName)
        {
            var promptBuilder = new StringBuilder();
            //promptBuilder.AppendLine("```");
            promptBuilder.AppendLine("//c# code");
            promptBuilder.AppendLine(code);
            promptBuilder.AppendLine("//Explain the c# code. Use plain language.");
            promptBuilder.Append("//");
            //promptBuilder.AppendLine("```");
            try
            {
                //ConsoleExtensions.WriteLine("Completion Test:", ConsoleColor.DarkCyan);
                var prompt = promptBuilder.ToString().Replace("\r\n", "\n");

                var completionCreateRequest = new CompletionCreateRequest()
                {
                    Prompt = prompt,
                    MaxTokens = 1024,
                    Temperature = 0,
                    N = 1,
                    BestOf = 1,
                    Echo = false,
                    StopAsList = new List<string> { "//c# code", "//Explain ", "\n\n\n\n" },
                    TopP = 1,
                    FrequencyPenalty = 1.5f,
                    
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
