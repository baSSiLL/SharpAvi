namespace SharpAvi
{
    /// <summary>Identifiers of various codecs.</summary>
    public static class CodecIds
    {
        /// <summary>Identifier used for non-compressed data.</summary>
        public static readonly FourCC Uncompressed = new FourCC(0);

        /// <summary>Motion JPEG.</summary>
        public static readonly FourCC MotionJpeg = new FourCC("MJPG");

        /// <summary>Microsoft MPEG-4 V3.</summary>
        public static readonly FourCC MicrosoftMpeg4V3 = new FourCC("MP43");

        /// <summary>Microsoft MPEG-4 V2.</summary>
        public static readonly FourCC MicrosoftMpeg4V2 = new FourCC("MP42");

        /// <summary>Xvid MPEG-4.</summary>
        public static readonly FourCC Xvid = new FourCC("XVID");

        /// <summary>DivX MPEG-4.</summary>
        public static readonly FourCC DivX = new FourCC("DIVX");

        /// <summary>x264 H.264/MPEG-4 AVC.</summary>
        public static readonly FourCC X264 = new FourCC("X264");
    }
}
