using System;
using TextPad.Model;
using TextPad.Services.Interop;

namespace TextPad.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly Settings settings_;

        public event EventHandler DefaultCharsetChanged;

        public SettingsService()
        {
            settings_ = Settings.Load();
        }

        public Charset DefaultCharset
        {
            get { return settings_.DefaultCharset; }
            set
            {
                if (settings_.DefaultCharset != value)
                {
                    settings_.DefaultCharset = value;
                    settings_.Save();
                    RaiseDefaultCharsetChanged();
                }
            }
        }

        private void RaiseDefaultCharsetChanged()
        {
            var handler = DefaultCharsetChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
    }
}
