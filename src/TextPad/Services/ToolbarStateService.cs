using System;

using TextPad.Services.Interop;

namespace TextPad.Services
{
    public sealed class ToolbarStateService : IToolbarStateService
    {
        private bool isPaneOpen_ = false;
        private bool saveCommandEnabled_ = false;

        public event EventHandler IsPaneOpenChanged;
        public event EventHandler SaveCommandEnabledChanged;

        public bool IsPaneOpen
        {
            get { return isPaneOpen_; }
            set
            {
                if (isPaneOpen_ != value)
                {
                    isPaneOpen_ = value;
                    RaiseIsPaneOpenChanged();
                }
            }
        }

        public bool SaveCommandEnabled
        {
            get { return saveCommandEnabled_; }
            set
            {
                if (saveCommandEnabled_ != value)
                {
                    saveCommandEnabled_ = value;
                    RaiseSaveCommandEnabledChanged();
                }
            }
        }

        private void RaiseIsPaneOpenChanged()
        {
            RaiseEvent(IsPaneOpenChanged);
        }

        private void RaiseSaveCommandEnabledChanged()
        {
            RaiseEvent(SaveCommandEnabledChanged);
        }

        private void RaiseEvent(EventHandler handler)
        {
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
    }
}
