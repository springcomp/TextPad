using System.Text;

namespace TextPad.Encodings
{
    /// <summary>
    /// A simple <see cref="DecoderFallback" /> implementation
    /// that replaces invalid byte sequence into the '•' character.
    /// Also, it provides a property whose value might be useful
    /// to a caller in order to know whether any replacement did
    /// in fact take place.
    /// </summary>
    public sealed class CustomReplacementDecoderFallback : DecoderFallback
    {
        /// <summary>
        /// Use buit-in replacement fallback strategy
        /// </summary>
        private readonly DecoderFallback decoder_ = DecoderFallback.ReplacementFallback;

        /// <summary>
        /// Set by the internal decoder fallback implementation
        /// if a character replacement did, in fact, occur.
        /// </summary>
        public bool WasTriggered { get; internal set; }

        /// <summary>
        /// Reset the 'WasTriggered' state.
        /// </summary>
        public void Reset()
        {
            WasTriggered = false;
        }

        public override int MaxCharCount
        {
            get { return decoder_.MaxCharCount; }
        }

        public override DecoderFallbackBuffer CreateFallbackBuffer()
        {
            // called by the decoder when it encounters
            // the first byte that it is unable to successfully decode

            WasTriggered = true;

            return decoder_.CreateFallbackBuffer();
        }
    }
}
