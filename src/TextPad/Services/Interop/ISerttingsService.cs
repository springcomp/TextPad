using System;
using TextPad.Model;

namespace TextPad.Services.Interop
{
    public interface ISettingsService
    {
        event EventHandler DefaultCharsetChanged;

        Charset DefaultCharset { get; set; }
    }
}
