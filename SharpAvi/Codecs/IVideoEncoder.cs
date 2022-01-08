using System;

namespace SharpAvi.Codecs
{
    /// <summary>
    /// Encoder for video AVI stream.
    /// </summary>
    public interface IVideoEncoder
    {
        /// <summary>Codec ID.</summary>
        FourCC Codec { get; }

        /// <summary>
        /// Number of bits per pixel in encoded image.
        /// </summary>
        BitsPerPixel BitsPerPixel { get; }

        /// <summary>
        /// Determines the amount of space needed in the destination buffer for storing the encoded data of a single frame.
        /// </summary>
        int MaxEncodedSize { get; }

        /// <summary>
        /// Encodes video frame.
        /// </summary>
        /// <param name="source">
        /// Frame bitmap data. The expected bitmap format is BGR32 top-to-bottom. Alpha component is not used.
        /// </param>
        /// <param name="srcOffset">
        /// Start offset of the frame data in the <paramref name="source"/>.
        /// Expected length of the data is determined by the parameters specified when instantinating the encoder.
        /// </param>
        /// <param name="destination">
        /// Buffer for storing the encoded frame data.
        /// </param>
        /// <param name="destOffset">
        /// Start offset of the encoded data in the <paramref name="destination"/> buffer.
        /// There should be enough space till the end of the buffer, see <see cref="MaxEncodedSize"/>.
        /// </param>
        /// <param name="isKeyFrame">
        /// When the method returns, contains the value indicating whether this frame was encoded as a key frame.
        /// </param>
        /// <returns>
        /// The actual number of bytes written to the <paramref name="destination"/> buffer.
        /// </returns>
        int EncodeFrame(byte[] source, int srcOffset, byte[] destination, int destOffset, out bool isKeyFrame);

#if NET5_0_OR_GREATER
        /// <summary>
        /// Encodes video frame.
        /// </summary>
        /// <param name="source">
        /// Frame bitmap data. The expected bitmap format is BGR32 top-to-bottom. Alpha component is not used.
        /// </param>
        /// <param name="destination">
        /// Buffer for storing the encoded frame data.
        /// </param>
        /// <param name="isKeyFrame">
        /// When the method returns, contains the value indicating whether this frame was encoded as a key frame.
        /// </param>
        /// <returns>
        /// The actual number of bytes written to the <paramref name="destination"/> buffer.
        /// </returns>
        int EncodeFrame(ReadOnlySpan<byte> source, Span<byte> destination, out bool isKeyFrame);
#endif
    }
}
