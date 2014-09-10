using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using SharpAvi.Codecs;

namespace SharpAvi.Output
{
    /// <summary>
    /// Wrapper on the <see cref="IAviAudioStream"/> object to provide encoding.
    /// </summary>
    internal class EncodingAudioStreamWrapper : IAviAudioStream, IAviStreamInternal, IDisposable
    {
        private readonly IAviAudioStream baseStream;
        private readonly IAviStreamInternal baseStreamInternal;
        private readonly IAudioEncoder encoder;
        private readonly bool ownsEncoder;
        private byte[] encodedBuffer;

        public EncodingAudioStreamWrapper(IAviAudioStream baseStream, IAudioEncoder encoder, bool ownsEncoder)
        {
            Contract.Requires(baseStream != null);
            Contract.Requires(baseStream is IAviStreamInternal);
            Contract.Requires(encoder != null);

            this.baseStream = baseStream;
            this.baseStreamInternal = (IAviStreamInternal)baseStream;
            this.encoder = encoder;
            this.ownsEncoder = ownsEncoder;

            encoder.InitializeStream(baseStream);
        }

        public void Dispose()
        {
            if (ownsEncoder)
            {
                var encoderDisposable = encoder as IDisposable;
                if (encoderDisposable != null)
                {
                    encoderDisposable.Dispose();
                }
            }
        }

        /// <summary>
        /// Number of channels in this audio stream.
        /// </summary>
        /// <remarks>
        /// The value of this property is defined by the the encoder used.
        /// When accessing the corresponding property of the <see cref="IAviAudioStream"/> interface, its setter
        /// throws a <see cref="NotSupportedException"/>.
        /// </remarks>
        public int ChannelCount
        {
            get { return baseStream.ChannelCount; }
        }

        int IAviAudioStream.ChannelCount
        {
            get { return ChannelCount; }
            set { ThrowPropertyDefinedByEncoder(); }
        }

        /// <summary>
        /// Sample rate, in samples per second (herz).
        /// </summary>
        /// <remarks>
        /// The value of this property is defined by the the encoder used.
        /// When accessing the corresponding property of the <see cref="IAviAudioStream"/> interface, its setter
        /// throws a <see cref="NotSupportedException"/>.
        /// </remarks>
        public int SamplesPerSecond
        {
            get { return baseStream.SamplesPerSecond; }
        }

        int IAviAudioStream.SamplesPerSecond
        {
            get { return SamplesPerSecond; }
            set { ThrowPropertyDefinedByEncoder(); }
        }

        /// <summary>
        /// Number of bits per sample per single channel (usually 8 or 16).
        /// </summary>
        /// <remarks>
        /// The value of this property is defined by the the encoder used.
        /// When accessing the corresponding property of the <see cref="IAviAudioStream"/> interface, its setter
        /// throws a <see cref="NotSupportedException"/>.
        /// </remarks>
        public int BitsPerSample
        {
            get { return baseStream.BitsPerSample; }
        }

        int IAviAudioStream.BitsPerSample
        {
            get { return BitsPerSample; }
            set { ThrowPropertyDefinedByEncoder(); }
        }

        /// <summary>
        /// Format of the audio data.
        /// </summary>
        /// <remarks>
        /// The value of this property is defined by the the encoder used.
        /// When accessing the corresponding property of the <see cref="IAviAudioStream"/> interface, its setter
        /// throws a <see cref="NotSupportedException"/>.
        /// </remarks>
        public short Format
        {
            get { return baseStream.Format; }
        }

        short IAviAudioStream.Format
        {
            get { return Format; }
            set { ThrowPropertyDefinedByEncoder(); }
        }

        /// <summary>
        /// Average byte rate of the stream.
        /// </summary>
        /// <remarks>
        /// The value of this property is defined by the the encoder used.
        /// When accessing the corresponding property of the <see cref="IAviAudioStream"/> interface, its setter
        /// throws a <see cref="NotSupportedException"/>.
        /// </remarks>
        public int BytesPerSecond
        {
            get { return baseStream.BytesPerSecond; }
        }

        int IAviAudioStream.BytesPerSecond
        {
            get { return BytesPerSecond; }
            set { ThrowPropertyDefinedByEncoder(); }
        }

        /// <summary>
        /// Size in bytes of minimum item of data in the stream.
        /// </summary>
        /// <remarks>
        /// The value of this property is defined by the the encoder used.
        /// When accessing the corresponding property of the <see cref="IAviAudioStream"/> interface, its setter
        /// throws a <see cref="NotSupportedException"/>.
        /// </remarks>
        public int Granularity
        {
            get { return baseStream.Granularity; }
        }

        int IAviAudioStream.Granularity
        {
            get { return Granularity; }
            set { ThrowPropertyDefinedByEncoder(); }
        }

        /// <summary>
        /// Extra data defined by a specific format which should be added to the stream header.
        /// </summary>
        /// <remarks>
        /// The value of this property is defined by the the encoder used.
        /// When accessing the corresponding property of the <see cref="IAviAudioStream"/> interface, its setter
        /// throws a <see cref="NotSupportedException"/>.
        /// </remarks>
        public byte[] FormatSpecificData
        {
            get { return baseStream.FormatSpecificData; }
        }

        byte[] IAviAudioStream.FormatSpecificData
        {
            get { return FormatSpecificData; }
            set { ThrowPropertyDefinedByEncoder(); }
        }

        /// <summary>
        /// Encodes and writes a block of audio data.
        /// </summary>
        public void WriteBlock(byte[] data, int startIndex, int length)
        {
            EnsureBufferIsSufficient(length);
            var encodedLength = encoder.EncodeBlock(data, startIndex, length, encodedBuffer, 0);
            baseStream.WriteBlock(encodedBuffer, 0, encodedLength);
        }

        /// <summary>
        /// Number of blocks written.
        /// </summary>
        public int BlocksWritten
        {
            get { return baseStream.BlocksWritten; }
        }

        /// <summary>
        /// Sequential number of the stream.
        /// </summary>
        public int Index
        {
            get { return baseStream.Index; }
        }

        /// <summary>
        /// Name of the stream.
        /// </summary>
        public string Name
        {
            get { return baseStream.Name; }
            set { baseStream.Name = value; }
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

        public FourCC StreamType
        {
            get { return baseStreamInternal.StreamType; }
        }

        public FourCC ChunkId
        {
            get { return baseStreamInternal.ChunkId; }
        }

        public void Freeze()
        {
            baseStreamInternal.Freeze();
        }

        public void WriteHeader()
        {
            baseStreamInternal.WriteHeader();
        }

        public void WriteFormat()
        {
            baseStreamInternal.WriteFormat();
        }
    }
}
