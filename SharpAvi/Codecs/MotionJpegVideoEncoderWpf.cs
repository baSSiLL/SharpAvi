using System.Diagnostics.Contracts;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System;
using System.Threading;

namespace SharpAvi.Codecs
{
    /// <summary>
    /// Encodes frames in Motion JPEG format.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The implementation relies on <see cref="JpegBitmapEncoder"/>.
    /// All calls to <see cref="EncodeFrame"/> should be made on the same thread. However, this thread may differ from the thread 
    /// on which the encoder instance was created.
    /// </para>
    /// <para>
    /// This encoder is not fully conformant to the Motion JPEG standard, as each encoded frame is a full JPEG picture 
    /// with its own Huffman tables, and not those fixed Huffman tables defined by the Motion JPEG standard. 
    /// However, (at least most) modern decoders for Motion JPEG properly handle this situation.
    /// This also produces a little overhead on the file size.
    /// </para>
    /// </remarks>
    public class MotionJpegVideoEncoderWpf : IVideoEncoder
    {
        private readonly Int32Rect rect;
        private readonly int quality;
        private readonly ThreadLocal<WriteableBitmap> bitmap;

        /// <summary>
        /// Creates a new instance of <see cref="MotionJpegVideoEncoderWpf"/>.
        /// </summary>
        /// <param name="width">Frame width.</param>
        /// <param name="height">Frame height.</param>
        /// <param name="quality">
        /// Compression quality in the range [1..100].
        /// Less values mean less size and lower image quality.
        /// </param>
        public MotionJpegVideoEncoderWpf(int width, int height, int quality)
        {
            Contract.Requires(width > 0);
            Contract.Requires(height > 0);
            Contract.Requires(1 <= quality && quality <= 100);

            rect = new Int32Rect(0, 0, width, height);
            this.quality = quality;

            bitmap = new ThreadLocal<WriteableBitmap>(
                () => new WriteableBitmap(rect.Width, rect.Height, 96, 96, PixelFormats.Bgr32, null),
                false);
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
                return Math.Max(rect.Width * rect.Height * 4, 1024);
            }
        }

        /// <summary>
        /// Encodes a frame.
        /// </summary>
        /// <seealso cref="IVideoEncoder.EncodeFrame"/>
        public int EncodeFrame(byte[] source, int srcOffset, byte[] destination, int destOffset, out bool isKeyFrame)
        {
            // Creating bitmap here (not in constructor) to allow encoding on a separate thread
            bitmap.Value.WritePixels(rect, source, rect.Width * 4, srcOffset);

            var encoderImpl = new JpegBitmapEncoder
            {
                QualityLevel = quality
            };
            encoderImpl.Frames.Add(BitmapFrame.Create(bitmap.Value));

            using (var stream = new MemoryStream(destination))
            {
                stream.Position = srcOffset;
                encoderImpl.Save(stream);
                stream.Flush();
                var length = stream.Position - srcOffset;
                stream.Close();

                isKeyFrame = true;

                return (int)length;
            }
        }

        #endregion
    }
}
