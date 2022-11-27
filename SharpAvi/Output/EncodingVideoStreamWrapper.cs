using System;
using System.Threading.Tasks;
using SharpAvi.Codecs;
using SharpAvi.Utilities;

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
            Argument.IsNotNull(encoder, nameof(encoder));

            this.encoder = encoder;
            this.ownsEncoder = ownsEncoder;
            encodedBuffer = new byte[encoder.MaxEncodedSize];
        }

        public override void Dispose()
        {
            if (ownsEncoder)
            {
                (encoder as IDisposable)?.Dispose();
            }

            base.Dispose();
        }


        /// <summary> Video codec. </summary>
        public override FourCC Codec
        {
            get => encoder.Codec;
            set => ThrowPropertyDefinedByEncoder();
        }

        /// <summary> Bits per pixel. </summary>
        public override BitsPerPixel BitsPerPixel
        {
            get => encoder.BitsPerPixel;
            set => ThrowPropertyDefinedByEncoder();
        }

        public override byte[] BitmapInfoHeader
        {
            get
            {
                if (encoder is IVideoEncoderExtraData extraData)
                {
                    return extraData.BitmapInfoHeader;
                }
                else
                {
                    return BaseStream.BitmapInfoHeader;
                }
            }
            set => ThrowPropertyDefinedByEncoder();
        }

        /// <summary>Encodes and writes a frame.</summary>
        public override void WriteFrame(bool isKeyFrame, byte[] frameData, int startIndex, int length)
        {
            Argument.IsNotNull(frameData, nameof(frameData));
            Argument.IsNotNegative(startIndex, nameof(startIndex));
            Argument.IsPositive(length, nameof(length));
            Argument.ConditionIsMet(startIndex + length <= frameData.Length, "End offset exceeds the length of frame data.");

            // Prevent accessing encoded buffer by multiple threads simultaneously
            lock (syncBuffer)
            {
                length = encoder.EncodeFrame(frameData, startIndex, encodedBuffer, 0, out isKeyFrame);
                base.WriteFrame(isKeyFrame, encodedBuffer, 0, length);
            }
        }

        public override Task WriteFrameAsync(bool isKeyFrame, byte[] frameData, int startIndex, int length) 
            => throw new NotSupportedException("Asynchronous writes are not supported.");

#if NET5_0_OR_GREATER
        public override void WriteFrame(bool isKeyFrame, ReadOnlySpan<byte> frameData)
        {
            Argument.Meets(frameData.Length > 0, nameof(frameData), "Cannot write an empty frame.");

            // Prevent accessing encoded buffer by multiple threads simultaneously
            lock (syncBuffer)
            {
                var encodedLength = encoder.EncodeFrame(frameData, encodedBuffer.AsSpan(), out isKeyFrame);
                base.WriteFrame(isKeyFrame, encodedBuffer.AsSpan(0, encodedLength));
            }
        }
#endif

        public override void PrepareForWriting()
        {
            // Set properties of the base stream
            BaseStream.Codec = encoder.Codec;
            BaseStream.BitsPerPixel = encoder.BitsPerPixel;
            if (encoder is IVideoEncoderExtraData extraData)
            {
                BaseStream.BitmapInfoHeader = extraData.BitmapInfoHeader;
            }

            base.PrepareForWriting();
        }


        private void ThrowPropertyDefinedByEncoder() 
            => throw new NotSupportedException("The value of the property is defined by the encoder.");
    }
}
