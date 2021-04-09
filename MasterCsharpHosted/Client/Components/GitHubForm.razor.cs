using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MasterCsharpHosted.Shared;
using Microsoft.AspNetCore.Components;

namespace MasterCsharpHosted.Client.Components
{
    public partial class GitHubForm
    {
        [Inject]
        private AppState AppState { get; set; }
        [Inject]
        private PublicClient PublicClient { get; set; }
        class FormModel
        {
            [Required]
            public string Organization { get; set; }
            [Required]
            public string Repo { get; set; }
            [Required]
            public string FileName { get; set; }

            public bool InRoot { get; set; } = true;
            public List<string> Folders => new(3){$"/{Folder1}", $"/{Folder2}", $"/{Folder3}" };
            public string Folder1 { get; set; }
            public string Folder2 { get; set; }
            public string Folder3 { get; set; }


        }

        private int folderCount = 0;

        private void AddFolder()
        {
            if (folderCount > 3) return;
            folderCount += 1;
            //ghFrm.Folders.Add(string.Empty);
            StateHasChanged();
        }

        private void RemoveFolder()
        {
            if (folderCount == 0) return;
            folderCount -= 1;
            //ghFrm.Folders.RemoveAt(ghFrm.Folders.Count - 1);
            StateHasChanged();
        }
        private FormModel ghFrm = new();
        private async void HandleValidSubmit()
        {
            string enforcedFileExt = ghFrm.FileName.EndsWith(".cs") ? ghFrm.FileName : $"{ghFrm.FileName}.cs";
            ghFrm.FileName = enforcedFileExt;
            var folder1 = string.IsNullOrWhiteSpace(ghFrm.Folder1) ? "" : $"/{ghFrm.Folder1}";
            var folder2 = string.IsNullOrWhiteSpace(ghFrm.Folder2) ? "" : $"/{ghFrm.Folder2}";
            var folder3 = string.IsNullOrWhiteSpace(ghFrm.Folder3) ? "" : $"/{ghFrm.Folder3}";
            var fullPath = folder1 + folder2 + folder3 + "/" + ghFrm.FileName;
            AppState.Snippet =
                await PublicClient.GetFromPublicRepo(ghFrm.Organization, ghFrm.Repo, fullPath);
            StateHasChanged();
        }
    }
}
