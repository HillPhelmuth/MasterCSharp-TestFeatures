using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MasterCsharpHosted.Shared.Annotations;

namespace MasterCsharpHosted.Shared
{
    public class AppState : INotifyPropertyChanged
    {
        private string _currentOutput = "";
        private string _content;
        private string _snippet;
        public event PropertyChangedEventHandler PropertyChanged;
       

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

        public void AddLineToOutput(string output)
        {
            CurrentOutput += Environment.NewLine + output;
            //NotifyOutputChange();
        }

        public void ClearOutput()
        {
            CurrentOutput = "";
            //NotifyOutputChange();
        }

        public void UpdateSnippet(string snippet)
        {
            Snippet = snippet;
            //  NotifyUpdateSnippet();
        }
        //public void NotifyOutputChange()
        //{
        //    OnOutputChange?.Invoke(CurrentOutput);
        //}

        //public void NotifyUpdateSnippet()
        //{
        //    OnSnippetChange?.Invoke(Snippet);
        //}

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
