using System;
using System.Diagnostics.Contracts;
using SharpAvi.Codecs;

namespace SharpAvi.Output
{
    /// <summary>
    /// Wrapper on the <see cref="IAviVideoStreamInternal"/> object to provide encoding.
    /// </summary>
    internal class EncodingVideoStreamWrapper : VideoStreamWrapperBase
    {
        private readonly IVideoEncoder encoder;
        private readonly bool ownsEncoder;
        private readonly byte[] encodedBuffer;
        private readonly object syncBuffer = new object();

        /// <summary>
        /// Creates a new instance of <see cref="EncodingVideoStreamWrapper"/>.
        /// </summary>
        /// <param name="baseStream">Video stream to be wrapped.</param>
        /// <param name="encoder">Encoder to be used.</param>
        /// <param name="ownsEncoder">Whether to dispose the encoder.</param>
        public EncodingVideoStreamWrapper(IAviVideoStreamInternal baseStream, IVideoEncoder encoder, bool ownsEncoder)
            : base(baseStream)
        {
            Contract.Requires(baseStream != null);
            Contract.Requires(encoder != null);

            this.encoder = encoder;
            this.ownsEncoder = ownsEncoder;
            encodedBuffer = new byte[encoder.MaxEncodedSize];
        }

        public override void Dispose()
        {
            if (ownsEncoder)
            {
                var encoderDisposable = encoder as IDisposable;
                if (encoderDisposable != null)
                {
                    encoderDisposable.Dispose();
                }
            }

            base.Dispose();
        }


        /// <summary> Video codec. </summary>
        public override FourCC Codec
        {
            get { return encoder.Codec; }
            set
            {
                ThrowPropertyDefinedByEncoder();
            }
        }

        /// <summary> Bits per pixel. </summary>
        public override BitsPerPixel BitsPerPixel
        {
            get { return encoder.BitsPerPixel; }
            set
            {
                ThrowPropertyDefinedByEncoder();
            }
        }

        /// <summary>Encodes and writes a frame.</summary>
        public override void WriteFrame(bool isKeyFrame, byte[] frameData, int startIndex, int count)
        {
            // Prevent accessing encoded buffer by multiple threads simultaneously
            lock (syncBuffer)
            {
                count = encoder.EncodeFrame(frameData, startIndex, encodedBuffer, 0, out isKeyFrame);
                base.WriteFrame(isKeyFrame, encodedBuffer, 0, count);
            }
        }

#if FX45
        public override System.Threading.Tasks.Task WriteFrameAsync(bool isKeyFrame, byte[] frameData, int startIndex, int length)
        {
            throw new NotSupportedException("Asynchronous writes are not supported.");
        }
#else
        public override IAsyncResult BeginWriteFrame(bool isKeyFrame, byte[] frameData, int startIndex, int length, AsyncCallback userCallback, object stateObject)
        {
            throw new NotSupportedException("Asynchronous writes are not supported.");
        }

        public override void EndWriteFrame(IAsyncResult asyncResult)
        {
            throw new NotSupportedException("Asynchronous writes are not supported.");
        }
#endif

        public override void PrepareForWriting()
        {
            // Set properties of the base stream
            BaseStream.Codec = encoder.Codec;
            BaseStream.BitsPerPixel = encoder.BitsPerPixel;

            base.PrepareForWriting();
        }


        private void ThrowPropertyDefinedByEncoder()
        {
            throw new NotSupportedException("The value of the property is defined by the encoder.");
        }
    }
}
