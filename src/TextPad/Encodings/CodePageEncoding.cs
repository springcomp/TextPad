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
        /// If a codePoint is not in this array and is above 255 then it is not a valid character.
        /// </summary>
        protected IDictionary<int /* codePoint */, byte> charToBytes_ = new Dictionary<int, byte>();

        protected CodePageEncoding(int codePage, string name, string webName)
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

                var valid = false;
                byte b = (byte) '?';

                if (charToBytes_.ContainsKey(code))
                {
                    b = charToBytes_[code];
                    valid = true;
                }

                else if (code <= 255)
                {
                    b = (byte)code;

                    // codePoint is a byte excluding those that are already mapped

                    if (!charToBytes_.Values.Contains(b))
                        valid = true;
                }

                if (!valid && EncoderFallback != null)
                {
                    //var buffer = EncoderFallback.CreateFallbackBuffer();
                    //while (buffer.Remaining > 0)
                    //{
                    //    var next = buffer.GetNextChar();
                    //}
               }

                bytes[charIndex + index] = b;
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
