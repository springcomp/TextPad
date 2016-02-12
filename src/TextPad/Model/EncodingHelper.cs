using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextPad.Encodings;

namespace TextPad.Model
{
    public static class EncodingHelper
    {
        /// <summary>
        /// Return a <see cref="Encoding" /> object associated with the specified <see cref="Charset" />.
        /// The Encoding object is associated with custom implementations for the
        /// <see cref"EncoderFallback" /> and <see cref"DecoderFallback" /> objects so
        /// that it is possible to determine whether any replacement occurred during decoding.
        /// </summary>
        /// <param name="charset"></param>
        /// <returns></returns>
        public static Encoding GetEncoding(Charset charset)
        {
            var decoderFallback = new CustomReplacementDecoderFallback();
            var encoderFallback = EncoderFallback.ReplacementFallback;

            switch (charset)
            {
                case Charset.Greece:
                    {
                        var encoding = Windows1253Encoding.Create(
                            encoderFallback
                            , decoderFallback)
                            ;
                        return encoding;
                    }

                case Charset.CentralEasternEurope:
                    {
                        var encoding = Windows1250Encoding.Create(
                            encoderFallback
                            , decoderFallback)
                            ;
                        return encoding;
                    }

                case Charset.WesternEurope:
                    {
                        var encoding = Windows1252Encoding.Create(
                            encoderFallback
                            , decoderFallback)
                            ;
                        return encoding;
                    }

                case Charset.UnicodeBe:
                    {
                        var cp = Encoding.BigEndianUnicode.CodePage;
                        var encoding = Encoding.GetEncoding(cp
                            , EncoderFallback.ReplacementFallback
                            , decoderFallback
                            );
                        return encoding;
                    }

                case Charset.UnicodeLe:
                    {
                        var cp = Encoding.Unicode.CodePage;
                        var encoding = Encoding.GetEncoding(cp
                            , EncoderFallback.ReplacementFallback
                            , decoderFallback
                            );
                        return encoding;
                    }

                case Charset.UTF8:
                default:
                    {
                        var cp = Encoding.UTF8.CodePage;
                        var encoding = Encoding.GetEncoding(cp
                            , EncoderFallback.ReplacementFallback
                            , decoderFallback
                            );
                        return encoding;
                    }
            }
        }

        /// <summary>
        /// Returns whether the previous decoding was successfull.
        /// i.e. no replacement fallback did occur.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public static bool DecodedSuccessfully(this Encoding encoding)
        {
            var decoderFallback = encoding.DecoderFallback;
            var customFallback = decoderFallback as CustomReplacementDecoderFallback;
            if (customFallback == null)
                return true;

            return !customFallback.WasTriggered;
        }

        /// <summary>
        /// Reset the state of the decoder fallback.
        /// </summary>
        /// <param name="encoding"></param>
        public static void ResetDecoderFallback(this Encoding encoding)
        {
            var decoderFallback = encoding.DecoderFallback;
            var customFallback = decoderFallback as CustomReplacementDecoderFallback;
            if (customFallback == null)
                return;

            customFallback.Reset();
        }
    }
}
