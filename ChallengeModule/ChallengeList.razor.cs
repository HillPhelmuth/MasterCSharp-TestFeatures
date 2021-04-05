using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MasterCsharpHosted.Shared;
using Microsoft.AspNetCore.Components;

namespace ChallengeModule
{
    public partial class ChallengeList
    {
        [Inject]
        private AppState AppState { get; set; }
        private List<ChallengeModel> _challenges = new();
        private readonly List<ChallengeView> _challengeViews = new();
        protected override Task OnInitializedAsync()
        {
            var challenges = ChallengeModel.GetChallengesFromFile();
            _challengeViews.Add(new ChallengeView("Very Easy", challenges.FindAll(x => x.Difficulty == Difficulty.Easiest), 1, true));
            _challengeViews.Add(new ChallengeView("Easy", challenges.FindAll(x => x.Difficulty == Difficulty.Easier), 2));
            _challengeViews.Add(new ChallengeView("Sorta Easy", challenges.FindAll(x => x.Difficulty == Difficulty.Easy), 3));
            _challengeViews.Add(new ChallengeView("Not Too Easy", challenges.FindAll(x => x.Difficulty == Difficulty.Mid), 4));
            _challengeViews.Add(new ChallengeView("Not At All Easy", challenges.FindAll(x => x.Difficulty == Difficulty.Hard), 5));
            return base.OnInitializedAsync();
        }

        private void Open(ChallengeView view)
        {
            foreach (var chal in _challengeViews)
            {
                chal.IsActive = false;
            }

            view.IsActive = !view.IsActive;
            StateHasChanged();
        }
        private class ChallengeView
        {
            public ChallengeView(string header, List<ChallengeModel> challenges, int order = 0, bool isActive = false)
            {
                Header = header;
                Challenges = challenges;
                Order = order;
                IsActive = isActive;
            }
            public int Order { get; }
            public bool IsActive { get; set; }
            public string Header { get; }
            public List<ChallengeModel> Challenges { get; }
        }
    }
}
