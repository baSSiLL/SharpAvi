using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpAvi
{
    /// <summary>
    /// Contains codes of some popular wave formats.
    /// </summary>
    public static class AudioFormats
    {
        /// <summary>
        /// Unknown format.
        /// </summary>
        public static readonly short Unknown = 0x0000;

        /// <summary>
        /// Pulse-code modulation (PCM).
        /// </summary>
        public static readonly short Pcm = 0x0001;

        /// <summary>
        /// MPEG Layer 3 (MP3).
        /// </summary>
        public static readonly short Mp3 = 0x0055;
    }
}
