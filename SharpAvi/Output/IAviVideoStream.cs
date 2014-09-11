using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace SharpAvi.Output
{
    /// <summary>
    /// Video stream of AVI file.
    /// </summary>
    /// <remarks>
    /// After the first invocation of <see cref="WriteFrame"/> no properties of the stream can be changed.
    /// </remarks>
    [ContractClass(typeof(Contracts.AviVideoStreamContract))]
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
        Task WriteFrameAsync(bool isKeyFrame, byte[] frameData, int startIndex, int length);

        /// <summary>
        /// Number of frames written.
        /// </summary>
        int FramesWritten { get; }
    }

    
    namespace Contracts
    {
        [ContractClassFor(typeof(IAviVideoStream))]
        internal abstract class AviVideoStreamContract : IAviVideoStream
        {
            public int Width
            {
                get 
                { 
                    Contract.Ensures(Contract.Result<int>() > 0);
                    throw new NotImplementedException();
                }
                set
                {
                    Contract.Requires(value > 0);
                }
            }

            public int Height
            {
                get
                {
                    Contract.Ensures(Contract.Result<int>() > 0);
                    throw new NotImplementedException();
                }
                set
                {
                    Contract.Requires(value > 0);
                }
            }

            public BitsPerPixel BitsPerPixel
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    Contract.Requires(Enum.IsDefined(typeof(BitsPerPixel), value));
                }
            }

            public FourCC Codec
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                }
            }

            public void WriteFrame(bool isKeyFrame, byte[] frameData, int startIndex, int length)
            {
                Contract.Requires(frameData != null);
                Contract.Requires(startIndex >= 0);
                Contract.Requires(length >= 0);
                Contract.Requires(startIndex + length <= frameData.Length);
            }

            public Task WriteFrameAsync(bool isKeyFrame, byte[] frameData, int startIndex, int length)
            {
                Contract.Requires(frameData != null);
                Contract.Requires(startIndex >= 0);
                Contract.Requires(length >= 0);
                Contract.Requires(startIndex + length <= frameData.Length);
                Contract.Ensures(Contract.Result<Task>() != null);
                throw new NotImplementedException();
            }

            public int FramesWritten
            {
                get
                {
                    Contract.Ensures(Contract.Result<int>() >= 0);
                    throw new NotImplementedException();
                }
            }

            public int Index
            {
                get { throw new NotImplementedException(); }
            }

            public string Name
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                }
            }

            public FourCC StreamType
            {
                get { throw new NotImplementedException(); }
            }
        }
    }
}
