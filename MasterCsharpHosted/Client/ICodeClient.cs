using System.Collections.Generic;
using System.Threading.Tasks;
using MasterCsharpHosted.Shared;

namespace MasterCsharpHosted.Client
{
    public interface ICodeClient : IPublicClient
    {
        Task<string> GetFromPublicRepo(string org, string repo, string filePath);
        Task<SyntaxTreeInfo> GetAnalysis(string code);
        Task<List<SimpleSyntaxTree>> GetSimpleAnalysis(string code);
    }
    public interface IUserClient
    {
        Task<AppUser> GetOrAddUser(string userName);
        Task<bool> UpdateUser(AppUser user);
    }

    public interface IChallengeClient
    {
        Task<CodeOutputModel> SubmitChallenge(ChallengeModel challenge);
    }
}