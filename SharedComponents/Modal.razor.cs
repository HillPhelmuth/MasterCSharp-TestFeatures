using MasterCsharpHosted.Shared;
using MasterCsharpHosted.Shared.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace SharedComponents
{
    public partial class Modal
    {
        [Parameter]
        public bool IsOpen { get; set; }
        [Parameter]
        public EventCallback<bool> IsOpenChanged { get; set; }
        [Parameter]
        public Location ModalLocation { get; set; }

        [Parameter]
        public RenderFragment? ChildContent { get; set; }
        [Inject]
        private ModalService ModalService { get; set; } = default!;
        private Type? ComponentType { get; set; }
        private ModalParameters? ModalParameters { get; set; }
        private ModalOptions ModalOptions { get; set; } = new();
        private ModalConfirmOptions? ModalConfirmOptions { get; set; }
        
        public void Reset()
        {
            ChildContent = null;
            ComponentType = null;
            ModalConfirmOptions = null;
        }
        protected override Task OnInitializedAsync()
        {
            Console.WriteLine("Modal Initialized");
            ModalService.OnOpenComponent += Open;
            ModalService.OnOpenFragment += OpenFragment;
            ModalService.OnModalClose += Close;
            ModalService.OnConfirmOpen += OpenConfirm;
            return base.OnInitializedAsync();
        }

        private void OpenFragment(RenderFragment<ModalService> childContent, ModalParameters? parameters = null,
            ModalOptions? options = null)
        {
            Console.WriteLine("ModalService OnOpenFragment handled in Modal.razor");
            Reset();
            ChildContent = childContent.Invoke(ModalService);
            ModalParameters = parameters;
            ModalOptions = options ?? new ModalOptions();
            IsOpen = true;
            StateHasChanged();
        }
        private void Open(Type type, ModalParameters? parameters = null, ModalOptions? options = null)
        {
            Console.WriteLine("ModalService OnOpenComponent handled in Modal.razor");
            Reset();
            ComponentType = type;
            ModalParameters = parameters;
            ModalOptions = options ?? new ModalOptions();
            IsOpen = true;
            StateHasChanged();
        }

        private void OpenConfirm(ModalConfirmOptions confirmOptions)
        {
            Console.WriteLine("ModalService OpenConfirm handled in Modal.razor");
            Reset();
            ModalOptions = new ModalOptions
            {
                CloseOnOuterClick = false,
                Location = Location.Center
            };
            ModalConfirmOptions = confirmOptions;
            IsOpen = true;
            StateHasChanged();
        }

        private void Close(ModalResults? results = null)
        {
            Console.WriteLine("ModalService OnClose handled in Modal.razor");
            //ModalService.Close(results);
            IsOpen = false;
            StateHasChanged();
        }
        private void CloseSelf()
        {
            ModalService.CloseSelf();
            //StateHasChanged();
        }

        private void ConfirmClose(bool confirm)
        {
            Console.WriteLine("ModalService OnClose handled in Modal.razor");
            ModalService.CloseConfirm(confirm);
            IsOpen = false;
            StateHasChanged();
        }
        private void OutClick()
        {
            if (ModalOptions.CloseOnOuterClick)
            {
                CloseSelf();
            }
        }

    }
}
