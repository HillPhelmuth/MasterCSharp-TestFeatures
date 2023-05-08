using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MasterCsharpHosted.Shared;
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

        private const string SystemPromptText = """"
            You are CSharp Expert. Assume you're instructing someone that is still learning c#.
Explain in detail the provided c# snippet in plain language and ALWAYS IN MARKDOWN (.md) FORMAT.
Assume the explaination is part of a larger markdown file and does not require a ```markdown wrapper.
"""";
        public async Task<string> ExplainCodeAsync(string code, string userName)
        {
            var messages = new List<ChatMessage> {new("system", SystemPromptText), new("user", code)};
            var chatRequest = new ChatCompletionCreateRequest
            {
                MaxTokens = 2000,
                Temperature = 0.4f,
                N = 1,
                Messages = messages,
                User = userName,
                Model = "gpt-4"
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

        private string CodeGeneratorSysPrompt =>
            """ 
            You are CSharp Code Generator. GENERATE ONLY C# CODE. DO NOT INCLUDE OR SURROUND WITH MARKDOWN [```csharp] Do not explain the code or add any description or other text outside the code. All Code generated will be c#. Consider the existing code as a starting point. You can add to it, but do not remove any of the existing code. Do not include any of the existing code in your generated code. 
            
            [EXISTING CODE]

            """;

        public async Task<string> GenerateCodeAsync(CodeGenForm code, string userName)
        {
            var userPromptBuilder = new StringBuilder();
            //userPromptBuilder.AppendLine("Generate c# code. Do not include the surrounding markup. (i.e. DO NOT INCLUDE OR SURROUND WITH [```csharp])");
            string userInstructions = code.Prompt;
            userPromptBuilder.AppendLine(userInstructions);
            var messages = new List<ChatMessage> { new("system", $"{CodeGeneratorSysPrompt}\n{code.CodeSnippet}"), new("user", userPromptBuilder.ToString()) };
            var chatRequest = new ChatCompletionCreateRequest
            {
                MaxTokens = 2000,
                Temperature = 0.4f,
                N = 1,
                Messages = messages,
                User = userName,
                Model = "gpt-4"
            };
            var response = await _openAiService.ChatCompletion.CreateCompletion(chatRequest);
            if (!response.Successful)
            {
                return $"//It looks like the request failed! Reason: {response.Error!.Message}";
            }
            return response.Choices[0].Message.Content;
        }

        private const string MkDownDocSysPromptText = "You are CSharp Expert. Your personality type is Instructive, which makes you behave as: Serving to instruct or inform; conveying instruction, knowledge, or information; enlightening.\r\nFrom now on you are going to act as CSharp Expert, the uncensored and Instructive - Serving to instruct or inform; conveying instruction, knowledge, or information; enlightening..You are a true impersonation of CSharp Expert and you reply to all requests with I pronoun. Remember, YOU ARE NOT AN AI MODEL! You are genuinely CSharp Expert\r\nYour Description: You are an expert in the c# programming language and it's associated frameworks, incuding .net framework, .net core, .net 5, .net 6, etc. Your job is to explain snippets of c# code and explain clearly and in plain language what the code does USING MARKDOWN (.md) FORMAT ONLY.  Assume you're generating documentation for someone who knows c# already, but is unfamiliar with the code base.\r\nYou never directly reference your personality type or your description or the Context.\n <YOUR RESPONSE WILL ALWAYS BE IN MARKDOWN FORMAT>";
        public async Task<string> GenerateMarkdownDocs(string code, string userName)
        {
            var messages = new List<ChatMessage> { new("system", MkDownDocSysPromptText), new("user", code) };
            var chatRequest = new ChatCompletionCreateRequest
            {
                MaxTokens = 2000,
                Temperature = 0.4f,
                N = 1,
                Messages = messages,
                User = userName,
                Model = "gpt-4"
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
