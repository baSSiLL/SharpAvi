using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;

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
            Contract.Requires(index >= 0);
            Contract.Requires(writeHandler != null);

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

        public void WriteBlock(byte[] buffer, int startIndex, int count)
        {
            writeHandler.WriteAudioBlock(this, buffer, startIndex, count);
            System.Threading.Interlocked.Increment(ref blocksWritten);
        }

#if FX45
        public System.Threading.Tasks.Task WriteBlockAsync(byte[] data, int startIndex, int length)
        {
            throw new NotSupportedException("Asynchronous writes are not supported.");
        }
#else
        public IAsyncResult BeginWriteBlock(byte[] data, int startIndex, int length, AsyncCallback userCallback, object stateObject)
        {
            throw new NotSupportedException("Asynchronous writes are not supported.");
        }

        public void EndWriteBlock(IAsyncResult asyncResult)
        {
            throw new NotSupportedException("Asynchronous writes are not supported.");
        }
#endif

        public int BlocksWritten
        {
            get { return blocksWritten; }
        }


        public override FourCC StreamType
        {
            get { return KnownFourCCs.StreamTypes.Audio; }
        }

        protected override FourCC GenerateChunkId()
        {
 	        return KnownFourCCs.Chunks.AudioData(Index);
        }

        public override void WriteHeader()
        {
            writeHandler.WriteStreamHeader(this);
        }

        public override void WriteFormat()
        {
            writeHandler.WriteStreamFormat(this);
        }
    }
}
