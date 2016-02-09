using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextPad.Encodings
{
    /// <summary>
    /// A base class to implement single-byte character set encodings.
    /// </summary>
    public abstract class CodePageEncoding : Encoding
    {
        private int codePage_;
        private string encodingName_;
        private string webName_;

        /// <summary>
        /// A simple array mapping byte number to character
        /// </summary>
        protected char[] byteToChars_ = new char[] { };

        /// <summary>
        /// A simple dictionary that maps codePoints to bytes.
        /// All codePoints that are not in this array should map to the corresponding byte character.
        /// If a codePoint is not in this array and is above 255 then it is not a valid character for this encoding.
        /// </summary>
        protected IDictionary<char, byte> charToBytes_ = new Dictionary<char, byte>();

        protected CodePageEncoding(int codePage, string name, string webName)
            : this(codePage, name, webName, EncoderFallback.ReplacementFallback, DecoderFallback.ReplacementFallback)
        {
            codePage_ = codePage;
            encodingName_ = name;
            webName_ = webName;
        }

        protected CodePageEncoding(int codePage, string name, string webName, EncoderFallback encoderFallback, DecoderFallback decoderFallback)
            : base(codePage, encoderFallback, decoderFallback)
        {
            codePage_ = codePage;
            encodingName_ = name;
            webName_ = webName;
        }

        public override String EncodingName { get { return encodingName_; } }
        public override Int32 CodePage { get { return codePage_; } }
        public override String WebName { get { return webName_; } }

        public override bool IsSingleByte { get { return true; } }

        public override int GetByteCount(char[] chars, int index, int count)
        {
            return count;
        }

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            return count;
        }

        public override int GetMaxByteCount(int charCount)
        {
            return charCount;
        }

        public override int GetMaxCharCount(int byteCount)
        {
            return byteCount;
        }

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            System.Diagnostics.Debug.Assert(bytes.Length >= charCount);

            for (var index = charIndex; index < charIndex + charCount; index++)
            {                
                var c = chars[index];
                var code = (int)c;

                if (charToBytes_.ContainsKey(c))
                {
                    var b = charToBytes_[c];
                    bytes[charIndex + index] = b;
                    continue;
                }

                else if (code <= 255)
                {
                    var b = (byte)code;

                    // codePoint is a byte excluding those that are already mapped

                    if (!charToBytes_.Values.Contains(b))
                    {
                        bytes[charIndex + index] = b;
                        continue;
                    }
                }

                // if no valid characters could be converted
                // use the configured fallback mechanism

                if (EncoderFallback != null)
                {
                    System.Diagnostics.Debug.Assert(EncoderFallback.MaxCharCount == 1);

                    var fallbackBuffer = EncoderFallback.CreateFallbackBuffer();
                    if (fallbackBuffer.Fallback(c, index))
                    {
                        var next = fallbackBuffer.GetNextChar();

                        // recursively use this implementation
                        // to convert the replacement char to
                        // its corresponding byte

                        var buffer = new byte[1];
                        if (GetBytes(new[] { next, }, 0, 1, buffer, 0) == 1)
                        {
                            var b = buffer[0];
                            bytes[charIndex + index] = b;
                            continue;
                        }

                        // if the replacement character cannot
                        // be mapped, simply fallback to a '?'.
                        // (in practice, this should not happen)

                        else
                        {
                            var b = (byte)'?';
                            bytes[charIndex + index] = b;
                            continue;
                        }
                    }
                }

                // otherwise, just use a '?' character
                // (again, in practice, this should not
                // happen since a fallback should always
                // be configured on the Encoding.

                else
                {
                    var b = (byte)'?';
                    bytes[charIndex + index] = b;
                }
            }

            return charCount;
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            System.Diagnostics.Debug.Assert(chars.Length >= byteCount);
            System.Diagnostics.Debug.Assert(byteToChars_.Length == 256);

            for (var index = byteIndex; index < byteIndex + byteCount; index++)
            {
                var b = bytes[index];
                chars[charIndex + index] = byteToChars_[b];
            }

            return byteCount;
        }
    }
}
