using System;
using System.Diagnostics.Contracts;

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

        /// <summary>Writes a frame to the stream.</summary>
        /// <param name="isKeyFrame">Is this frame a key frame?</param>
        /// <param name="frameData">Array containing the frame data.</param>
        /// <param name="startIndex">Index of the first byte of the frame data.</param>
        /// <param name="count">Length of the frame data.</param>
        void WriteFrame(bool isKeyFrame, byte[] frameData, int startIndex, int count);
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
                    Contract.Ensures(Contract.Result<int>() >= 0);
                    throw new NotImplementedException();
                }
                set
                {
                    Contract.Requires(value >= 0);
                }
            }

            public int Height
            {
                get
                {
                    Contract.Ensures(Contract.Result<int>() >= 0);
                    throw new NotImplementedException();
                }
                set
                {
                    Contract.Requires(value >= 0);
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

            public void WriteFrame(bool isKeyFrame, byte[] frameData, int startIndex, int count)
            {
                Contract.Requires(frameData != null);
                Contract.Requires(startIndex >= 0);
                Contract.Requires(count >= 0);
                Contract.Requires(startIndex + count <= frameData.Length);
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
        }
    }
}
