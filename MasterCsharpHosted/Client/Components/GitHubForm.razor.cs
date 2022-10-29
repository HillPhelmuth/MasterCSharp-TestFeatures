using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MasterCsharpHosted.Shared;
using MasterCsharpHosted.Shared.Services;
using Microsoft.AspNetCore.Components;

namespace MasterCsharpHosted.Client.Components
{
    public partial class GitHubForm
    {
        [Inject]
        private AppState AppState { get; set; }
        [Inject]
        private ICodeClient PublicClient { get; set; }
        [Inject]
        private ModalService ModalService { get; set; }

        private class FormModel
        {
            [Required]
            public string Organization { get; set; }
            [Required]
            public string Repo { get; set; }
            [Required]
            public string FileName { get; set; }

            public bool InRoot { get; set; } = true;
            public List<Folder> Folders { get; } = new();
       
            public string Folder1 { get; set; }
            public string Folder2 { get; set; }
            public string Folder3 { get; set; }


        }

        private class Folder
        {
            public int Index { get; init; }
            public string Value { get; set; }
        }

        private int folderCount = 0;

        private void AddFolder()
        {
            //if (folderCount > 3) return;
            folderCount += 1;
            _ghFrm.Folders.Add(new Folder(){Index = folderCount});
            StateHasChanged();
        }

        private void RemoveFolder()
        {
            if (folderCount == 0) return;
            folderCount -= 1;
            var last = _ghFrm.Folders.LastOrDefault();
            if (last == null) return;
            _ghFrm.Folders.Remove(last);
            StateHasChanged();
        }
        private readonly FormModel _ghFrm = new();
        private async void HandleValidSubmit()
        {
            var enforcedFileExt = _ghFrm.FileName.EndsWith(".cs") ? _ghFrm.FileName : $"{_ghFrm.FileName}.cs";
            _ghFrm.FileName = enforcedFileExt;
            var folder1 = string.IsNullOrWhiteSpace(_ghFrm.Folder1) ? "" : $"/{_ghFrm.Folder1}";
            var folder2 = string.IsNullOrWhiteSpace(_ghFrm.Folder2) ? "" : $"/{_ghFrm.Folder2}";
            var folder3 = string.IsNullOrWhiteSpace(_ghFrm.Folder3) ? "" : $"/{_ghFrm.Folder3}";
            var fullPath =
                $"/{string.Join("/", _ghFrm.Folders.OrderBy(x => x.Index).Where(x => !string.IsNullOrWhiteSpace(x.Value)).Select(x => x.Value))}/{_ghFrm.FileName}";
            //var fullPath = folder1 + folder2 + folder3 + "/" + _ghFrm.FileName;
            //AppState.Snippet =
            //    await PublicClient.GetFromPublicRepo(_ghFrm.Organization, _ghFrm.Repo, fullPath);
            //StateHasChanged();
            ModalService.Close(new ModalResults(true, new ModalParameters { { "Organization", _ghFrm.Organization }, { "Repo", _ghFrm.Repo }, { "FullPath", fullPath } }));
        }
    }
}
