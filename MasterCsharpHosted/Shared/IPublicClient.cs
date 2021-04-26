using System.Threading.Tasks;

namespace MasterCsharpHosted.Shared
{
    public interface IPublicClient
    {
        public Task<string> CompileCodeAsync(string code);
        public Task<string> GetFromGithubRepo(string fileName);
    }
}