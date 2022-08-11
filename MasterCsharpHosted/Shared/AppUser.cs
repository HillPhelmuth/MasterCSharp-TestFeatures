using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterCsharpHosted.Shared
{
    public class AppUser
    {
        private ICollection<UserSnippet> _snippets;
        private ICollection<CompletedChallenge> _completedChallenges;

        public int Id { get; set; }
        public string UserName { get; set; }
        public bool IsAuthenticated { get; set; }

        public ICollection<UserSnippet> Snippets
        {
            get => _snippets ?? new List<UserSnippet>();
            set => _snippets = value;
        }

        public ICollection<CompletedChallenge> CompletedChallenges 
        { 
            get => _completedChallenges ?? new List<CompletedChallenge>(); 
            set => _completedChallenges = value; 
        }
    }

    public class UserSnippet
    {
        public UserSnippet() { }
        public UserSnippet(string name, string code)
        {
            Name = name;
            Code = code;
        }
        public UserSnippet(string name, string code, string description)
        {
            Name = name;
            Code = code;
            Description = description;
        }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
    }

    public record CompletedChallenge
    {
        public CompletedChallenge() { }
        public CompletedChallenge(string challengeName, string solution)
        {
            ChallengeName = challengeName;
            Solution = solution;
        }

        public string ChallengeName { get; set; }
        public string Solution { get; set; }
    }

}
