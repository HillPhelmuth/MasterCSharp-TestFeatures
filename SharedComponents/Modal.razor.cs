using Microsoft.AspNetCore.Components;

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
        public RenderFragment ChildContent { get; set; }

        private void Close()
        {
            IsOpen = false;
            IsOpenChanged.InvokeAsync(IsOpen);
        }

    }

    public enum Location
    {
        Center, Left, Right, TopLeft, TopRight
    }
}
