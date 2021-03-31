using System.Threading.Tasks;

namespace MasterCsharpHosted.Shared
{
    public interface IPublicClient
    {
        Task<string> CompileCodeAsync(string code);
        Task<string> GetFromGithubRepo(string fileName);
    }
}