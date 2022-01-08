using System.Diagnostics.Contracts;

namespace SharpAvi.Codecs
{
    /// <summary>
    /// Flip bitmap vertically without compression (from top-down to bottom-up).
    /// </summary>
    public class UniversalUncompressedVideoEncoder : IVideoEncoder
    {
        private readonly int _width;
        private readonly int _height;
        private readonly BitsPerPixel _bitsPerPixel;
        private readonly int _bytesPerPixel;


        public UniversalUncompressedVideoEncoder(int width, int height, BitsPerPixel bitsPerPixel)
        {
            Contract.Requires(width > 0);
            Contract.Requires(height > 0);

            _width = width;
            _height = height;
            _bitsPerPixel = bitsPerPixel;
            _bytesPerPixel = (int)bitsPerPixel / 8;
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
            get { return _bitsPerPixel; }
        }

        /// <summary>
        /// Maximum size of encoded frame.
        /// </summary>
        public int MaxEncodedSize
        {
            get { return _width * _height * _bytesPerPixel; }
        }

        /// <summary>
        /// Encodes a frame.
        /// </summary>
        /// <seealso cref="IVideoEncoder.EncodeFrame"/>
        public int EncodeFrame(byte[] source, int srcOffset, byte[] destination, int destOffset, out bool isKeyFrame)
        {
            BitmapUtils.FlipVertical(source, srcOffset, destination, destOffset, _height, _width * _bytesPerPixel);
            isKeyFrame = true;
            return MaxEncodedSize;
        }

        #endregion
    }
}