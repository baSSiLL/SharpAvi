using System;
using System.Diagnostics.Contracts;
using SharpAvi.Codecs;

namespace SharpAvi.Output
{
    // TODO: Support IDisposable to dispose the encoder?
    /// <summary>
    /// Wrapper on the <see cref="IAviVideoStream"/> object to provide encoding.
    /// </summary>
    public class EncodingVideoStreamWrapper : IAviVideoStream
    {
        private readonly IAviVideoStream stream;
        private readonly IVideoEncoder encoder;
        private readonly byte[] encodedBuffer;

        /// <summary>
        /// Creates a new instance of <see cref="EncodingVideoStreamWrapper"/>.
        /// </summary>
        /// <param name="stream">Video stream to be wrapped.</param>
        /// <param name="encoder">Encoder to be used.</param>
        public EncodingVideoStreamWrapper(IAviVideoStream stream, IVideoEncoder encoder)
        {
            Contract.Requires(stream != null);
            Contract.Requires(encoder != null);

            this.stream = stream;
            this.encoder = encoder;
            encodedBuffer = new byte[encoder.MaxEncodedSize];

            stream.Codec = encoder.Codec;
            stream.BitsPerPixel = encoder.BitsPerPixel;
        }


        /// <summary>
        /// Sequential number of the stream.
        /// </summary>
        public int Index
        {
            get { return stream.Index; }
        }

        /// <summary>
        /// Name of the stream.
        /// </summary>
        public string Name
        {
            get { return stream.Name; }
            set { stream.Name = value; }
        }

        /// <summary> Video codec. </summary>
        /// <remarks>
        /// The value of this property is defined by the <see cref="IVideoEncoder.BitsPerPixel"/> property of the encoder.
        /// When accessing the corresponding property of the <see cref="IAviVideoStream"/> interface, its setter
        /// throws a <see cref="NotSupportedException"/>.
        /// </remarks>
        public FourCC Codec
        {
            get { return encoder.Codec; }
        }

        FourCC IAviVideoStream.Codec
        {
            get { return Codec; }
            set
            {
                ThrowPropertyDefinedByEncoder();
            }
        }

        /// <summary>Frame width.</summary>
        public int Width
        {
            get { return stream.Width; }
            set { stream.Width = value; }
        }

        /// <summary>Frame height.</summary>
        public int Height
        {
            get { return stream.Height; }
            set { stream.Height = value; }
        }

        /// <summary> Bits per pixel. </summary>
        /// <remarks>
        /// The value of this property is defined by the <see cref="IVideoEncoder.BitsPerPixel"/> property of the encoder.
        /// When accessing the corresponding property of the <see cref="IAviVideoStream"/> interface, its setter
        /// throws a <see cref="NotSupportedException"/>.
        /// </remarks>
        public BitsPerPixel BitsPerPixel
        {
            get { return encoder.BitsPerPixel; }
        }

        BitsPerPixel IAviVideoStream.BitsPerPixel
        {
            get { return BitsPerPixel; }
            set
            {
                ThrowPropertyDefinedByEncoder();
            }
        }

        /// <summary>Encodes and writes a frame.</summary>
        /// <remarks>
        /// When invoking the corresponding method of the <see cref="IAviVideoStream"/> interface, 
        /// the values of its parameters <c>isKeyFrame</c> and <c>count</c> are ignored.
        /// </remarks>
        /// <seealso cref="IAviVideoStream.WriteFrame"/>
        public void WriteFrame(byte[] frameData, int startIndex)
        {
            bool isKeyFrame;
            var count = encoder.EncodeFrame(frameData, startIndex, encodedBuffer, 0, out isKeyFrame);
            stream.WriteFrame(isKeyFrame, encodedBuffer, 0, count);
        }

        void IAviVideoStream.WriteFrame(bool isKeyFrame, byte[] frameData, int startIndex, int count)
        {
            WriteFrame(frameData, startIndex);
        }


        private void ThrowPropertyDefinedByEncoder()
        {
            throw new NotSupportedException("The value of the property is defined by the encoder.");
        }
    }
}
