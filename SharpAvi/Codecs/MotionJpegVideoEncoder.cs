using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace SharpAvi.Codecs
{
    /// <summary>
    /// Encodes frames in Motion JPEG format.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This encoder is not fully conformant to the Motion JPEG standard, as each encoded frame is a full JPEG picture 
    /// with its own Huffman tables, and not those fixed Huffman tables defined by the Motion JPEG standard. 
    /// However, (at least most) modern decoders for Motion JPEG properly handle this situation.
    /// This also produces a little overhead on the file size.
    /// </para>
    /// <para>
    /// Note for .NET 3.5:
    /// This encoder is designed for single-threaded use.
    /// </para>
    /// </remarks>
    public class MotionJpegVideoEncoder : IVideoEncoder
    {
        private readonly int width;

        private readonly int height;

        private readonly int quality;

        private readonly ImageCodecInfo codecInfo;

        /// <summary>
        /// Creates a new instance of <see cref="MotionJpegVideoEncoder"/>.
        /// </summary>
        /// <param name="width">Frame width.</param>
        /// <param name="height">Frame height.</param>
        /// <param name="quality">
        /// Compression quality in the range [1..100].
        /// Less values mean less size and lower image quality.
        /// </param>
        public MotionJpegVideoEncoder(int width, int height, int quality)
        {
            this.width = width;
            this.height = height;
            this.quality = quality;

            this.codecInfo = ImageCodecInfo.GetImageDecoders()
                .First(codec => codec.FormatID == ImageFormat.Jpeg.Guid);
        }

        #region IVideoEncoder Members

        /// <summary>Video codec.</summary>
        public FourCC Codec
        {
            get { return KnownFourCCs.Codecs.MotionJpeg; }
        }

        /// <summary>
        /// Number of bits per pixel in encoded image.
        /// </summary>
        public BitsPerPixel BitsPerPixel
        {
            get { return SharpAvi.BitsPerPixel.Bpp24; }
        }

        /// <summary>
        /// Maximum size of encoded frmae.
        /// </summary>
        public int MaxEncodedSize
        {
            get
            {
                // Assume that JPEG is always less than raw bitmap when dimensions are not tiny
                return Math.Max(this.width * this.height * 4, 1024);
            }
        }

        /// <summary>
        /// Encodes a frame.
        /// </summary>
        /// <seealso cref="IVideoEncoder.EncodeFrame"/>
        public int EncodeFrame(byte[] source, int srcOffset, byte[] destination, int destOffset, out bool isKeyFrame)
        {
            using (EncoderParameters encoderParameters = new EncoderParameters(1))
            using (EncoderParameter encoderParameter = new EncoderParameter(Encoder.Quality, (long)quality))
            {
                encoderParameters.Param[0] = encoderParameter;

                using (var stream = new MemoryStream(destination))
                {
                    stream.Position = destOffset;

                    IntPtr sourceData = Marshal.UnsafeAddrOfPinnedArrayElement(source, srcOffset);

                    using (var bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppArgb, sourceData))
                    {
                        bitmap.Save(stream, codecInfo, encoderParameters);
                    }

                    stream.Flush();
                    var length = stream.Position - destOffset;
                    stream.Close();

                    isKeyFrame = true;

                    return (int)length;
                }
            }
        }

        #endregion
    }
}
