using System;

namespace TextPad.Services.Interop
{
    public interface IToolbarStateService
    {
        event EventHandler IsPaneOpenChanged;
        event EventHandler SaveCommandEnabledChanged;

        bool IsPaneOpen { get; set; }
        bool SaveCommandEnabled { get; set; }
    }
}
