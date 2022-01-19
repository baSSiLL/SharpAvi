using SharpAvi.Utilities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using System;
using System.IO;

namespace SharpAvi.Codecs
{
    /// <summary>
    /// Encodes frames in Motion JPEG format.
    /// </summary>
    /// <remarks>
    /// The implementation relies on <see cref="JpegEncoder"/> from the <c>SixLabors.ImageSharp</c> package.
    /// </remarks>
    public sealed class MJpegImageSharpVideoEncoder : IVideoEncoder
    {
        private readonly int width;
        private readonly int height;
        private readonly JpegEncoder jpegEncoder;
#if NET5_0_OR_GREATER
        private readonly MemoryStream buffer;
#endif

        /// <summary>
        /// Creates a new instance of <see cref="MJpegImageSharpVideoEncoder"/>.
        /// </summary>
        /// <param name="width">Frame width.</param>
        /// <param name="height">Frame height.</param>
        /// <param name="quality">
        /// Compression quality in the range [1..100].
        /// Less values mean less size and lower image quality.
        /// </param>
        public MJpegImageSharpVideoEncoder(int width, int height, int quality)
        {
            Argument.IsPositive(width, nameof(width));
            Argument.IsPositive(height, nameof(height));
            Argument.IsInRange(quality, 1, 100, nameof(quality));

            this.width = width;
            this.height = height;

#if NET5_0_OR_GREATER
            buffer = new MemoryStream(MaxEncodedSize);
#endif
            jpegEncoder = new JpegEncoder()
            {
                Quality = quality
            };
        }

        /// <summary>Video codec.</summary>
        public FourCC Codec => CodecIds.MotionJpeg;

        /// <summary>
        /// Number of bits per pixel in encoded image.
        /// </summary>
        public BitsPerPixel BitsPerPixel => BitsPerPixel.Bpp24;

        /// <summary>
        /// Maximum size of encoded frmae.
        /// </summary>
        public int MaxEncodedSize => Math.Max(width * height * 3, 1024);

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

            int length;
            using (var stream = new MemoryStream(destination))
            {
                stream.Position = destOffset;
                length = LoadAndEncodeImage(source.AsSpan(srcOffset), stream);
            }

            isKeyFrame = true;
            return length;
        }

#if NET5_0_OR_GREATER
        /// <summary>
        /// Encodes a frame.
        /// </summary>
        public int EncodeFrame(ReadOnlySpan<byte> source, Span<byte> destination, out bool isKeyFrame)
        {
            Argument.ConditionIsMet(4 * width * height <= source.Length,
                "Source end offset exceeds the source length.");

            buffer.SetLength(0);
            var length = LoadAndEncodeImage(source, buffer);
            buffer.GetBuffer().AsSpan(0, length).CopyTo(destination);

            isKeyFrame = true;
            return length;
        }
#endif

        private int LoadAndEncodeImage(ReadOnlySpan<byte> source, Stream destination)
        {
            var startPosition = (int)destination.Position;
            using (var image = Image.LoadPixelData<SixLabors.ImageSharp.PixelFormats.Bgra32>(source, width, height))
            {
                jpegEncoder.Encode(image, destination);
            }
            destination.Flush();
            return (int)(destination.Position - startPosition);
        }
    }
}
