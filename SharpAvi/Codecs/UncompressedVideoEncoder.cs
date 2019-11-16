#if !NET35
using System.Diagnostics.Contracts;
#endif

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
        private readonly byte[] sourceBuffer;

        /// <summary>
        /// Creates a new instance of <see cref="UncompressedVideoEncoder"/>.
        /// </summary>
        /// <param name="width">Frame width.</param>
        /// <param name="height">Frame height.</param>
        public UncompressedVideoEncoder(int width, int height)
        {
#if !NET35
            Contract.Requires(width > 0);
            Contract.Requires(height > 0);
#endif

            this.width = width;
            this.height = height;
            // Scan lines in Windows bitmaps should be aligned by 4 bytes (DWORDs)
            this.stride = (width * 3 + 3) / 4 * 4;
            sourceBuffer = new byte[width * height * 4];
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
        /// <seealso cref="IVideoEncoder.EncodeFrame"/>
        public int EncodeFrame(byte[] source, int srcOffset, byte[] destination, int destOffset, out bool isKeyFrame)
        {
            BitmapUtils.FlipVertical(source, srcOffset, sourceBuffer, 0, height, width * 4);
            for (var i = 0; i < height; i++)
            {
                BitmapUtils.Bgr32ToBgr24(sourceBuffer, i * width * 4, destination, destOffset + i * stride, width);
            }
            isKeyFrame = true;
            return MaxEncodedSize;
        }

#endregion
    }
}
