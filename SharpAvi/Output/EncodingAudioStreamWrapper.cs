using SharpAvi.Codecs;
using System;
using System.Threading.Tasks;

namespace SharpAvi.Output
{
    /// <summary>
    /// Wrapper on the <see cref="IAviAudioStreamInternal"/> object to provide encoding.
    /// </summary>
    internal class EncodingAudioStreamWrapper : AudioStreamWrapperBase
    {
        private readonly IAudioEncoder encoder;
        private readonly bool ownsEncoder;
        private byte[] encodedBuffer;
        private readonly object syncBuffer = new object();

        public EncodingAudioStreamWrapper(IAviAudioStreamInternal baseStream, IAudioEncoder encoder, bool ownsEncoder)
            : base(baseStream)
        {
            Argument.IsNotNull(encoder, nameof(encoder));

            this.encoder = encoder;
            this.ownsEncoder = ownsEncoder;
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

        /// <summary>
        /// Number of channels in this audio stream.
        /// </summary>
        public override int ChannelCount
        {
            get { return encoder.ChannelCount; }
            set { ThrowPropertyDefinedByEncoder(); }
        }

        /// <summary>
        /// Sample rate, in samples per second (herz).
        /// </summary>
        public override int SamplesPerSecond
        {
            get { return encoder.SamplesPerSecond; }
            set { ThrowPropertyDefinedByEncoder(); }
        }

        /// <summary>
        /// Number of bits per sample per single channel (usually 8 or 16).
        /// </summary>
        public override int BitsPerSample
        {
            get { return encoder.BitsPerSample; }
            set { ThrowPropertyDefinedByEncoder(); }
        }

        /// <summary>
        /// Format of the audio data.
        /// </summary>
        public override short Format
        {
            get { return encoder.Format; }
            set { ThrowPropertyDefinedByEncoder(); }
        }

        /// <summary>
        /// Average byte rate of the stream.
        /// </summary>
        public override int BytesPerSecond
        {
            get { return encoder.BytesPerSecond; }
            set { ThrowPropertyDefinedByEncoder(); }
        }

        /// <summary>
        /// Size in bytes of minimum item of data in the stream.
        /// </summary>
        public override int Granularity
        {
            get { return encoder.Granularity; }
            set { ThrowPropertyDefinedByEncoder(); }
        }

        /// <summary>
        /// Extra data defined by a specific format which should be added to the stream header.
        /// </summary>
        public override byte[] FormatSpecificData
        {
            get { return encoder.FormatSpecificData; }
            set { ThrowPropertyDefinedByEncoder(); }
        }

        /// <summary>
        /// Encodes and writes a block of audio data.
        /// </summary>
        public override void WriteBlock(byte[] data, int startIndex, int length)
        {
            Argument.IsNotNull(data, nameof(data));
            Argument.IsNotNegative(startIndex, nameof(startIndex));
            Argument.IsPositive(length, nameof(length));
            Argument.ConditionIsMet(startIndex + length <= data.Length, "End offset exceeds the length of data.");

            // Prevent accessing encoded buffer by multiple threads simultaneously
            lock (syncBuffer)
            {
                EnsureBufferIsSufficient(length);
                var encodedLength = encoder.EncodeBlock(data, startIndex, length, encodedBuffer, 0);
                if (encodedLength > 0)
                {
                    base.WriteBlock(encodedBuffer, 0, encodedLength);
                }
            }
        }

        public override Task WriteBlockAsync(byte[] data, int startIndex, int length) 
            => throw new NotSupportedException("Asynchronous writes are not supported.");

#if NET5_0_OR_GREATER
        public override void WriteBlock(ReadOnlySpan<byte> data)
        {
            Argument.Meets(data.Length > 0, nameof(data), "Cannot write an empty block.");

            // Prevent accessing encoded buffer by multiple threads simultaneously
            lock (syncBuffer)
            {
                EnsureBufferIsSufficient(data.Length);
                var encodedLength = encoder.EncodeBlock(data, encodedBuffer.AsSpan());
                if (encodedLength > 0)
                {
                    base.WriteBlock(encodedBuffer.AsSpan(0, encodedLength));
                }
            }
        }

        public override Task WriteBlockAsync(ReadOnlyMemory<byte> data)
            => throw new NotSupportedException("Asynchronous writes are not supported.");
#endif

        public override void PrepareForWriting()
        {
            // Set properties of the base stream
            BaseStream.ChannelCount = ChannelCount;
            BaseStream.SamplesPerSecond = SamplesPerSecond;
            BaseStream.BitsPerSample = BitsPerSample;
            BaseStream.Format = Format;
            BaseStream.FormatSpecificData = FormatSpecificData;
            BaseStream.BytesPerSecond = BytesPerSecond;
            BaseStream.Granularity = Granularity;

            base.PrepareForWriting();
        }

        public override void FinishWriting()
        {
            // Prevent accessing encoded buffer by multiple threads simultaneously
            lock (syncBuffer)
            {
                // Flush the encoder
                EnsureBufferIsSufficient(0);
                var encodedLength = encoder.Flush(encodedBuffer, 0);
                if (encodedLength > 0)
                {
                    base.WriteBlock(encodedBuffer, 0, encodedLength);
                }
            }

            base.FinishWriting();
        }


        private void EnsureBufferIsSufficient(int sourceCount)
        {
            var maxLength = encoder.GetMaxEncodedLength(sourceCount);
            if (encodedBuffer != null && encodedBuffer.Length >= maxLength)
            {
                return;
            }

            var newLength = encodedBuffer == null ? 1024 : encodedBuffer.Length * 2;
            while (newLength < maxLength)
            {
                newLength *= 2;
            }

            encodedBuffer = new byte[newLength];
        }

        private void ThrowPropertyDefinedByEncoder()
        {
            throw new NotSupportedException("The value of the property is defined by the encoder.");
        }
    }
}
