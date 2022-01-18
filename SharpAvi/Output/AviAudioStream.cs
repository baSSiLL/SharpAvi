using SharpAvi.Format;
using SharpAvi.Utilities;
using System;
using System.Threading.Tasks;

namespace SharpAvi.Output
{
    internal class AviAudioStream : AviStreamBase, IAviAudioStreamInternal
    {
        private readonly IAviStreamWriteHandler writeHandler;
        private int channelCount = 1;
        private int samplesPerSecond = 44100;
        private int bitsPerSample = 8;
        private short format = AudioFormats.Unknown;
        private int bytesPerSecond = 44100;
        private int granularity = 1;
        private byte[] formatData;
        private int blocksWritten;


        public AviAudioStream(int index, IAviStreamWriteHandler writeHandler, 
            int channelCount, int samplesPerSecond, int bitsPerSample)
            : base(index)
        {
            Argument.IsNotNull(writeHandler, nameof(writeHandler));

            this.writeHandler = writeHandler;

            this.format = AudioFormats.Pcm;
            this.formatData = null;

            this.channelCount = channelCount;
            this.samplesPerSecond = samplesPerSecond;
            this.bitsPerSample = bitsPerSample;
            this.granularity = (bitsPerSample * channelCount + 7) / 8;
            this.bytesPerSecond = granularity * samplesPerSecond;
        }

        
        public int ChannelCount
        {
            get { return channelCount; }
            set
            {
                CheckNotFrozen();
                channelCount = value;
            }
        }

        public int SamplesPerSecond
        {
            get { return samplesPerSecond; }
            set
            {
                CheckNotFrozen();
                samplesPerSecond = value;
            }
        }

        public int BitsPerSample
        {
            get { return bitsPerSample; }
            set
            {
                CheckNotFrozen();
                bitsPerSample = value;
            }
        }

        public short Format
        {
            get { return format; }
            set
            {
                CheckNotFrozen();
                format = value;
            }
        }

        public int BytesPerSecond 
        {
            get { return bytesPerSecond; }
            set
            {
                CheckNotFrozen();
                bytesPerSecond = value;
            }
        }

        public int Granularity 
        {
            get { return granularity; }
            set
            {
                CheckNotFrozen();
                granularity = value;
            }
        }

        public byte[] FormatSpecificData
        {
            get { return formatData; }
            set
            {
                CheckNotFrozen();
                formatData = value;
            }
        }

        public void WriteBlock(byte[] data, int startIndex, int length)
        {
            Argument.IsNotNull(data, nameof(data));
            Argument.IsNotNegative(startIndex, nameof(startIndex));
            Argument.IsPositive(length, nameof(length));
            Argument.ConditionIsMet(startIndex + length <= data.Length, "End offset exceeds the length of data.");

#if NET5_0_OR_GREATER
            WriteBlock(data.AsSpan(startIndex, length));
#else
            writeHandler.WriteAudioBlock(this, data, startIndex, length);
            System.Threading.Interlocked.Increment(ref blocksWritten);
#endif
        }

        public Task WriteBlockAsync(byte[] data, int startIndex, int length)
            => throw new NotSupportedException("Asynchronous writes are not supported.");

#if NET5_0_OR_GREATER
        public void WriteBlock(ReadOnlySpan<byte> data)
        {
            Argument.Meets(data.Length > 0, nameof(data), "Cannot write an empty block.");

            writeHandler.WriteAudioBlock(this, data);
            System.Threading.Interlocked.Increment(ref blocksWritten);
        }

        public Task WriteBlockAsync(ReadOnlyMemory<byte> data)
            => throw new NotSupportedException("Asynchronous writes are not supported.");
#endif

        public int BlocksWritten => blocksWritten;


        public override FourCC StreamType => KnownFourCCs.StreamTypes.Audio;

        protected override FourCC GenerateChunkId() => KnownFourCCs.Chunks.AudioData(Index);

        public override void WriteHeader() => writeHandler.WriteStreamHeader(this);

        public override void WriteFormat() => writeHandler.WriteStreamFormat(this);
    }
}
