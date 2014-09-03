using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;

namespace SharpAvi.Output
{
    internal class AviAudioStream : AviStreamBase, IAviAudioStream
    {
        private readonly IAviStreamWriteHandler writeHandler;
        private int channelCount = 1;
        private int samplesPerSecond = 44100;
        private int bitsPerSample = 8;
        private short format = AudioFormats.Unknown;
        private byte[] formatData;


        public AviAudioStream(int index, IAviStreamWriteHandler writeHandler)
            : base(index)
        {
            Contract.Requires(index >= 0);
            Contract.Requires(writeHandler != null);

            this.writeHandler = writeHandler;
            BlocksWritten = 0;
        }

        
        #region IAviAudioStream implementation

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
            BlocksWritten++;
        }

        public int BlocksWritten
        {
            get;
            private set;
        }

        #endregion


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
