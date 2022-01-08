#if NET5_0_OR_GREATER
using JpegLibrary;
using SharpAvi.Utilities;
using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpAvi.Codecs
{
    /// <summary>
    /// Encodes frames in the Motion JPEG format.
    /// </summary>
    /// <remarks>
    /// This implementation relies on the <c>JpegLibrary</c> by yigolden.
    /// </remarks>
    public sealed class MotionJpegVideoEncoderJpegLibrary : IVideoEncoder
    {
        private readonly int width;
        private readonly int height;
        private readonly bool removeDht;
        private readonly byte[] ycbcr;
        private readonly ArrayBufferWriter<byte> writer = new();
        private readonly JpegEncoder jpegEncoder;

        /// <summary>
        /// Creates a new instance of <see cref="MotionJpegVideoEncoderJpegLibrary"/>.
        /// </summary>
        /// <param name="width">Frame width.</param>
        /// <param name="height">Frame height.</param>
        /// <param name="quality">
        /// Compression quality in the range [1..100].
        /// Less values mean less size and lower image quality.
        /// </param>
        /// <param name="removeDht">
        /// Whether to remove the DHT block (Huffman tables) from encoded JPEG frames.
        /// Set to <c>true</c> if your priority is the size of a file (saves about 400 bytes per frame),
        /// or keep the default <c>false</c> if your priority is the speed of encoding.
        /// </param>
        public MotionJpegVideoEncoderJpegLibrary(int width, int height, int quality, bool removeDht = true)
        {
            Argument.IsPositive(width, nameof(width));
            Argument.IsPositive(height, nameof(height));
            Argument.IsInRange(quality, 1, 100, nameof(quality));

            this.width = width;
            this.height = height;
            this.removeDht = removeDht;
            
            ycbcr = new byte[width * height * 3];
            jpegEncoder = CreateJpegEncoder(quality);
            jpegEncoder.SetOutput(writer);
        }

        private static JpegEncoder CreateJpegEncoder(int quality)
        {
            var encoder = new JpegEncoder();

            encoder.SetQuantizationTable(
                JpegStandardQuantizationTable.ScaleByQuality(
                    JpegStandardQuantizationTable.GetLuminanceTable(JpegElementPrecision.Precision8Bit, 0), quality));
            encoder.SetQuantizationTable(
                JpegStandardQuantizationTable.ScaleByQuality(
                    JpegStandardQuantizationTable.GetChrominanceTable(JpegElementPrecision.Precision8Bit, 1), quality));

            encoder.SetHuffmanTable(true, 0, JpegStandardHuffmanEncodingTable.GetLuminanceDCTable());
            encoder.SetHuffmanTable(false, 0, JpegStandardHuffmanEncodingTable.GetLuminanceACTable());
            encoder.SetHuffmanTable(true, 1, JpegStandardHuffmanEncodingTable.GetChrominanceDCTable());
            encoder.SetHuffmanTable(false, 1, JpegStandardHuffmanEncodingTable.GetChrominanceACTable());

            encoder.AddComponent(1, 0, 0, 0, 2, 2); // Y component
            encoder.AddComponent(2, 1, 1, 1, 1, 1); // Cb component
            encoder.AddComponent(3, 1, 1, 1, 1, 1); // Cr component

            return encoder;
        }

        #region IVideoEncoder Members

        /// <summary>Video codec.</summary>
        public FourCC Codec => KnownFourCCs.Codecs.MotionJpeg;

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

            return EncodeFrame(source.AsSpan(srcOffset), destination.AsSpan(destOffset), out isKeyFrame);
        }

        /// <summary>
        /// Encodes a frame.
        /// </summary>
        public int EncodeFrame(ReadOnlySpan<byte> source, Span<byte> destination, out bool isKeyFrame)
        {
            Argument.ConditionIsMet(4 * width * height <= source.Length,
                "Source end offset exceeds the source length.");

            BitmapUtils.Bgr32ToYCbCr8(source, ycbcr, width * height);

            // TODO: Encode directly to destination
            jpegEncoder.SetInputReader(new JpegBufferInputReader(width, height, 3, ycbcr));
            writer.Clear();
            jpegEncoder.Encode();

            if (removeDht)
            {
                CopyJpegWithoutHuffmanTables(writer.WrittenSpan, destination);
            }
            else
            {
                writer.WrittenSpan.CopyTo(destination);
            }

            isKeyFrame = true;
            return writer.WrittenSpan.Length;
        }

        private static void CopyJpegWithoutHuffmanTables(ReadOnlySpan<byte> source, Span<byte> destination)
        {
            const byte START_OF_SCAN = 0xDA;
            const byte DEFINE_HUFFMAN_TABLE = 0xC4;

            var excludeRange = new Range(0, 1);
            var sosIndex = FindJpegMarker(source, START_OF_SCAN);
            if (sosIndex >= 0)
            {
                var dhtIndex = FindJpegMarker(source.Slice(0, sosIndex), DEFINE_HUFFMAN_TABLE);
                if (dhtIndex >= 0)
                {
                    excludeRange = new Range(dhtIndex, sosIndex);
                }
            }

            source.Slice(0, excludeRange.Start.Value).CopyTo(destination);
            source.Slice(excludeRange.End.Value).CopyTo(destination.Slice(excludeRange.Start.Value));
        }

        private static int FindJpegMarker(ReadOnlySpan<byte> source, byte marker)
        {
            Span<byte> twoByteMarker = stackalloc byte[] { 0xFF, marker };
            return source.IndexOf(twoByteMarker);
        }

        #endregion

        private sealed class JpegBufferInputReader : JpegBlockInputReader
        {
            private readonly int width;
            private readonly int height;
            private readonly int componentCount;
            private readonly Memory<byte> buffer;

            public JpegBufferInputReader(int width, int height, int componentCount, Memory<byte> buffer)
            {
                this.width = width;
                this.height = height;
                this.componentCount = componentCount;
                this.buffer = buffer;
            }

            public override int Width => width;

            public override int Height => height;

            public override void ReadBlock(ref short blockRef, int componentIndex, int x, int y)
            {
                int width = this.width;
                int componentCount = this.componentCount;

                ref byte sourceRef = ref MemoryMarshal.GetReference(MemoryMarshal.AsBytes(buffer.Span));

                int blockWidth = Math.Min(width - x, 8);
                int blockHeight = Math.Min(height - y, 8);

                if (blockWidth != 8 || blockHeight != 8)
                {
                    Unsafe.As<short, JpegBlock8x8>(ref blockRef) = default;
                }

                for (int offsetY = 0; offsetY < blockHeight; offsetY++)
                {
                    int sourceRowOffset = (y + offsetY) * width + x;
                    ref short destinationRowRef = ref Unsafe.Add(ref blockRef, offsetY * 8);
                    for (int offsetX = 0; offsetX < blockWidth; offsetX++)
                    {
                        Unsafe.Add(ref destinationRowRef, offsetX) = Unsafe.Add(ref sourceRef, (sourceRowOffset + offsetX) * componentCount + componentIndex);
                    }
                }
            }
        }
    }
}
#endif