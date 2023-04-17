using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


namespace MasterCsharpHosted.Shared
{
    public class AppState : INotifyPropertyChanged
    {
        private string _currentOutput = "";
        private string _content;
        private string _snippet;
        private ChallengeModel _activeChallenge;
        private string _title = "Practice";
        private AppUser _currentUser;
        private SyntaxTreeInfo _syntaxTreeInfo;
        private string _editorTheme = "vs-dark";
        private string treeContent;
        private List<FullSyntaxTree> _fullSyntaxTrees = new();
        private CodeOutputModel challengeOutput;
        private string _explainContent;

        public event PropertyChangedEventHandler PropertyChanged;

        public AppUser CurrentUser
        {
            get => _currentUser;
            set
            {
                if (value == _currentUser) return;
                _currentUser = value;
                OnPropertyChanged();
            }
        }

        public SyntaxTreeInfo SyntaxTreeInfo
        {
            get => _syntaxTreeInfo;
            set
            {
                if (value == _syntaxTreeInfo) return;
                _syntaxTreeInfo = value;
                OnPropertyChanged();
            }
        }

        public string EditorTheme
        {
            get => _editorTheme;
            set
            {
                if (value == _editorTheme) return;
                _editorTheme = value;
                OnPropertyChanged();
            }
        }

        public string Snippet
        {
            get => _snippet;
            set
            {
                if (value == _snippet) return;
                _snippet = value;
                OnPropertyChanged();
            }
        }

        public string CurrentOutput
        {
            get => _currentOutput;
            set
            {
                if (value == _currentOutput) return;
                _currentOutput = value;
                OnPropertyChanged();
            }
        }

        public string Content
        {
            get => _content;
            set
            {
                if (value == _content) return;
                _content = value;
                OnPropertyChanged();
            }
        }

        public ChallengeModel ActiveChallenge
        {
            get => _activeChallenge;
            set
            {
                if (value == _activeChallenge) return;
                _activeChallenge = value;
                OnPropertyChanged();
            }
        }

        public string Title
        {
            get => _title;
            set
            {
                if (value == _title) return;
                _title = value;
                OnPropertyChanged();
            }
        }
        public string TreeContent
        {
            get => treeContent;
            set
            {
                treeContent = value;
                OnPropertyChanged();
            }
        }


        public List<FullSyntaxTree> FullSyntaxTrees
        {
            get => _fullSyntaxTrees;
            set
            {
                _fullSyntaxTrees = value;
                OnPropertyChanged();
            }
        }
        public CodeOutputModel ChallengeOutput
        {
            get => challengeOutput; 
            set
            {
                challengeOutput = value;
                OnPropertyChanged();
            }
        }

        public string ExplainContent
        {
            get => _explainContent;
            set
            {
                _explainContent = value;
                OnPropertyChanged();
            }
        }

        public void SetAnalysisResults(SyntaxTreeInfo syntaxTree, List<FullSyntaxTree> fullSyntax)
        {
            FullSyntaxTrees = fullSyntax;
            SyntaxTreeInfo = syntaxTree;
        }
        public void AddLineToOutput(string output) => CurrentOutput += $"{Environment.NewLine}{output}";

        public void ClearOutput() => CurrentOutput = "";

        public void UpdateSnippet(string snippet) => Snippet = snippet;

        public void SelectChallenge(ChallengeModel challenge)
        {
            Snippet = challenge.Snippet;
            ActiveChallenge = challenge;
        }

        public void DeleteUserSnippet(UserSnippet snippet)
        {
            if (CurrentUser?.Snippets == null || !CurrentUser.Snippets.Any()) return;
            CurrentUser.Snippets.Remove(snippet);
        }

        public event Action OnSubmitCode;

        public void SubmitCode()
        {
            OnSubmitCode?.Invoke();
        }
        public event Action<bool> OnShowCode;
        public void ShowCode(bool show) => OnShowCode?.Invoke(show);
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
