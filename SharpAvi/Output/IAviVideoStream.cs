using System;
using System.Diagnostics.Contracts;
#if FX45
using System.Threading.Tasks;
#endif

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

#if FX45
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
#else
        /// <summary>
        /// Asynchronously writes a frame to the stream.
        /// </summary>
        /// <param name="isKeyFrame">Is this frame a key frame?</param>
        /// <param name="frameData">Array containing the frame data.</param>
        /// <param name="startIndex">Index of the first byte of the frame data.</param>
        /// <param name="length">Length of the frame data.</param>
        /// <param name="userCallback">Callback to be invoked after asynchronous operation ends.</param>
        /// <param name="stateObject">User object that will be passed to <paramref name="userCallback"/>.</param>
        /// <returns><see cref="IAsyncResult"/> object representing this asynchronous operation.</returns>
        /// <remarks>
        /// The contents of <paramref name="frameData"/> should not be modified until this write operation ends.
        /// </remarks>
        /// <seealso cref="EndWriteFrame"/>
        IAsyncResult BeginWriteFrame(bool isKeyFrame, byte[] frameData, int startIndex, int length, AsyncCallback userCallback, object stateObject);

        /// <summary>
        /// Waits for asynchronous write operation to complete.
        /// </summary>
        /// <seealso cref="BeginWriteFrame"/>
        void EndWriteFrame(IAsyncResult asyncResult);
#endif

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

#if FX45
            public Task WriteFrameAsync(bool isKeyFrame, byte[] frameData, int startIndex, int length)
            {
                Contract.Requires(frameData != null);
                Contract.Requires(startIndex >= 0);
                Contract.Requires(length >= 0);
                Contract.Requires(startIndex + length <= frameData.Length);
                Contract.Ensures(Contract.Result<Task>() != null);
                throw new NotImplementedException();
            }
#else
            public IAsyncResult BeginWriteFrame(bool isKeyFrame, byte[] frameData, int startIndex, int length, AsyncCallback userCallback, object stateObject)
            {
                Contract.Requires(frameData != null);
                Contract.Requires(startIndex >= 0);
                Contract.Requires(length >= 0);
                Contract.Requires(startIndex + length <= frameData.Length);
                Contract.Ensures(Contract.Result<IAsyncResult>() != null);
                throw new NotImplementedException();
            }

            public void EndWriteFrame(IAsyncResult asyncResult)
            {
                Contract.Requires(asyncResult != null);
            }
#endif

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
