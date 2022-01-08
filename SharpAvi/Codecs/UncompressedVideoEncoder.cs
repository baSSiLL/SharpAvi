using SharpAvi.Utilities;
using System;

namespace SharpAvi.Codecs
{
    /// <summary>
    /// Encodes frames in BGR24 format without compression.
    /// </summary>
    /// <remarks>
    /// The main purpose of this encoder is to flip bitmap vertically (from top-down to bottom-up)
    /// and to convert pixel format to 24 bits.
    /// </remarks>
    public class UncompressedVideoEncoder : IVideoEncoder
    {
        private readonly int width;
        private readonly int height;
        private readonly int stride;

        /// <summary>
        /// Creates a new instance of <see cref="UncompressedVideoEncoder"/>.
        /// </summary>
        /// <param name="width">Frame width.</param>
        /// <param name="height">Frame height.</param>
        public UncompressedVideoEncoder(int width, int height)
        {
            Argument.IsPositive(width, nameof(width));
            Argument.IsPositive(height, nameof(height));

            this.width = width;
            this.height = height;
            // Scan lines in Windows bitmaps should be aligned by 4 bytes (DWORDs)
            this.stride = (width * 3 + 3) / 4 * 4;
        }

        #region IVideoEncoder Members

        /// <summary>Video codec.</summary>
        public FourCC Codec
        {
            get { return KnownFourCCs.Codecs.Uncompressed; }
        }

        /// <summary>
        /// Number of bits per pixel in encoded image.
        /// </summary>
        public BitsPerPixel BitsPerPixel
        {
            get { return BitsPerPixel.Bpp24; }
        }

        /// <summary>
        /// Maximum size of encoded frame.
        /// </summary>
        public int MaxEncodedSize
        {
            get { return stride * height; }
        }

        /// <summary>
        /// Encodes a frame.
        /// </summary>
        public int EncodeFrame(byte[] source, int srcOffset, byte[] destination, int destOffset, out bool isKeyFrame)
        {
            Argument.IsNotNull(source, nameof(source));
            Argument.IsNotNegative(srcOffset, nameof(srcOffset));
            Argument.ConditionIsMet(srcOffset + 4 * width * height <= source.Length,
                "Source end offset exceeds the source length.");
            Argument.IsNotNull(destination, nameof(destination));
            Argument.IsNotNegative(destOffset, nameof(destOffset));

            // Flip vertical and convert to 24 bpp
            for (var y = 0; y < height; y++)
            {
                var srcLineOffset = srcOffset + y * width * 4;
                var destLineOffset = destOffset + (height - 1 - y) * stride;
                BitmapUtils.Bgr32ToBgr24(source, srcLineOffset, destination, destLineOffset, width);
            }
            isKeyFrame = true;
            return MaxEncodedSize;
        }

#if NET5_0_OR_GREATER
        /// <summary>
        /// Encodes a frame.
        /// </summary>
        public int EncodeFrame(ReadOnlySpan<byte> source, Span<byte> destination, out bool isKeyFrame)
        {
            Argument.ConditionIsMet(4 * width * height <= source.Length,
                "Source end offset exceeds the source length.");

            // Flip vertical and convert to 24 bpp
            for (var y = 0; y < height; y++)
            {
                var srcOffset = y * width * 4;
                var destOffset = (height - 1 - y) * stride;
                BitmapUtils.Bgr32ToBgr24(source.Slice(srcOffset), destination.Slice(destOffset), width);
            }
            isKeyFrame = true;
            return MaxEncodedSize;
        }
#endif

        #endregion
    }
}
