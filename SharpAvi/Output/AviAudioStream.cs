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
        private int channelCount = 1;
        private int samplesPerSecond = 44100;
        private int bitsPerSample = 8;
        private short format = AudioFormats.Unknown;
        private byte[] formatData;


        public AviAudioStream(int index)
            : base(index)
        {
            Contract.Requires(index >= 0);
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

        #endregion


        public override FourCC StreamType
        {
            get { return KnownFourCCs.StreamTypes.Audio; }
        }

        protected override FourCC GenerateChunkId()
        {
 	        return KnownFourCCs.Chunks.AudioData(Index);
        }

        public override void WriteFormat(BinaryWriter writer)
        {
            // See WAVEFORMATEX structure
            writer.Write(Format);
            writer.Write((ushort)ChannelCount);
            writer.Write(SamplesPerSecond);
            if (Format == AudioFormats.Pcm)
            {
                var sampleByteSize = (ChannelCount * BitsPerSample) / 8;
                var byteRate = sampleByteSize * SamplesPerSecond;
                writer.Write((uint)byteRate);
                writer.Write((ushort)sampleByteSize);
                writer.Write((ushort)BitsPerSample);
            }
            else
            {
                // TODO: Get block size and byte rate info from format-specific strategy
                throw new NotImplementedException("Support for audio formats other than PCM is not currently implemented.");
            }
            if (FormatSpecificData != null)
            {
                writer.Write((ushort)FormatSpecificData.Length);
                writer.Write(FormatSpecificData);
            }
            else
            {
                writer.Write((ushort)0);
            }
        }
    }
}
