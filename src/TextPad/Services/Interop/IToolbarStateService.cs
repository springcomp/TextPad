using System;

namespace TextPad.Services.Interop
{
    public interface IToolbarStateService
    {
        event EventHandler SaveCommandEnabledChanged;

        bool SaveCommandEnabled { get; set; }
    }
}
