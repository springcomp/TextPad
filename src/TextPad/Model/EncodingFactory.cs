using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextPad.Encodings;

namespace TextPad.Model
{
    public sealed class EncodingFactory
    {
        public static Encoding GetEncoding(Charset charset)
        {
            switch (charset)
            {
                case Charset.UTF8:
                    return Encoding.UTF8;

                case Charset.UnicodeLe:
                    return Encoding.Unicode;

                case Charset.UnicodeBe:
                    return Encoding.BigEndianUnicode;

                case Charset.Western1252:
                    return Windows1252Encoding.Create();

                default:
                    return Encoding.UTF8;
            }
        }
    }
}
