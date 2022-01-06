using System.Threading.Tasks;

namespace SharpAvi.Output
{
    /// <summary>
    /// Video stream of AVI file.
    /// </summary>
    /// <remarks>
    /// After the first invocation of <see cref="WriteFrame"/> no properties of the stream can be changed.
    /// </remarks>
    public interface IAviVideoStream : IAviStream
    {
        /// <summary>Frame width.</summary>
        int Width { get; set; }

        /// <summary>Frame height.</summary>
        int Height { get; set; }

        /// <summary>
        /// Number of bits per pixel in the frame's image.
        /// </summary>
        BitsPerPixel BitsPerPixel { get; set; }

        /// <summary>
        /// ID of the codec used to encode the stream contents.
        /// </summary>
        FourCC Codec { get; set; }

        /// <summary>Writes a frame to the stream.</summary>
        /// <param name="isKeyFrame">Is this frame a key frame?</param>
        /// <param name="frameData">Array containing the frame data.</param>
        /// <param name="startIndex">Index of the first byte of the frame data.</param>
        /// <param name="length">Length of the frame data.</param>
        void WriteFrame(bool isKeyFrame, byte[] frameData, int startIndex, int length);

        /// <summary>Asynchronously writes a frame to the stream.</summary>
        /// <param name="isKeyFrame">Is this frame a key frame?</param>
        /// <param name="frameData">Array containing the frame data.</param>
        /// <param name="startIndex">Index of the first byte of the frame data.</param>
        /// <param name="length">Length of the frame data.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        /// <remarks>
        /// The contents of <paramref name="frameData"/> should not be modified until this write operation ends.
        /// </remarks>
        Task WriteFrameAsync(bool isKeyFrame, byte[] frameData, int startIndex, int length);

        /// <summary>
        /// Number of frames written.
        /// </summary>
        int FramesWritten { get; }
    }
}
