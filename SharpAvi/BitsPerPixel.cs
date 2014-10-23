namespace SharpAvi
{
    /// <summary>Number of bits per pixel.</summary>
    public enum BitsPerPixel
    {
        /// <summary>8 bits per pixel.</summary>
        /// <remarks>
        /// When used with uncompressed video streams,
        /// a grayscale palette is implied.
        /// </remarks>
        Bpp8 = 8,
        /// <summary>16 bits per pixel.</summary>
        Bpp16 = 16,
        /// <summary>24 bits per pixel.</summary>
        Bpp24 = 24,
        /// <summary>32 bits per pixel.</summary>
        Bpp32 = 32,
    }
}
