using System;

using TextPad.Services.Interop;

namespace TextPad.Services
{
    public sealed class ToolbarStateService : IToolbarStateService
    {
        private bool saveCommandEnabled_ = false;

        public event EventHandler SaveCommandEnabledChanged;

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

        private void RaiseSaveCommandEnabledChanged()
        {
            var handler = SaveCommandEnabledChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
    }
}
