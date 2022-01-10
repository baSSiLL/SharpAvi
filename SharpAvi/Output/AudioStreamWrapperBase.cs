using SharpAvi.Utilities;
using System;
using System.Threading.Tasks;

namespace SharpAvi.Output
{
    /// <summary>
    /// Base class for wrappers around <see cref="IAviAudioStreamInternal"/>.
    /// </summary>
    /// <remarks>
    /// Simply delegates all operations to wrapped stream.
    /// </remarks>
    internal abstract class AudioStreamWrapperBase : IAviAudioStreamInternal, IDisposable
    {
        protected AudioStreamWrapperBase(IAviAudioStreamInternal baseStream)
        {
            Argument.IsNotNull(baseStream, nameof(baseStream));

            this.BaseStream = baseStream;
        }

        protected IAviAudioStreamInternal BaseStream { get; }

        public virtual void Dispose() => (BaseStream as IDisposable)?.Dispose();

        public virtual int ChannelCount
        {
            get { return BaseStream.ChannelCount; }
            set { BaseStream.ChannelCount = value; }
        }

        public virtual int SamplesPerSecond
        {
            get { return BaseStream.SamplesPerSecond; }
            set { BaseStream.SamplesPerSecond = value; }
        }

        public virtual int BitsPerSample
        {
            get { return BaseStream.BitsPerSample; }
            set { BaseStream.BitsPerSample = value; }
        }

        public virtual short Format
        {
            get { return BaseStream.Format; }
            set { BaseStream.Format = value; }
        }

        public virtual int BytesPerSecond
        {
            get { return BaseStream.BytesPerSecond; }
            set { BaseStream.BytesPerSecond = value; }
        }

        public virtual int Granularity
        {
            get { return BaseStream.Granularity; }
            set { BaseStream.Granularity = value; }
        }

        public virtual byte[] FormatSpecificData
        {
            get { return BaseStream.FormatSpecificData; }
            set { BaseStream.FormatSpecificData = value; }
        }

        public virtual void WriteBlock(byte[] data, int startIndex, int length)
        {
            Argument.IsNotNull(data, nameof(data));
            Argument.IsNotNegative(startIndex, nameof(startIndex));
            Argument.IsPositive(length, nameof(length));
            Argument.ConditionIsMet(startIndex + length <= data.Length, "End offset exceeds the length of data.");

            BaseStream.WriteBlock(data, startIndex, length);
        }

        public virtual Task WriteBlockAsync(byte[] data, int startIndex, int length)
        {
            Argument.IsNotNull(data, nameof(data));
            Argument.IsNotNegative(startIndex, nameof(startIndex));
            Argument.IsPositive(length, nameof(length));
            Argument.ConditionIsMet(startIndex + length <= data.Length, "End offset exceeds the length of data.");

            return BaseStream.WriteBlockAsync(data, startIndex, length);
        }

#if NET5_0_OR_GREATER
        public virtual void WriteBlock(ReadOnlySpan<byte> data)
        {
            Argument.Meets(data.Length > 0, nameof(data), "Cannot write an empty block.");

            BaseStream.WriteBlock(data);
        }

        public virtual Task WriteBlockAsync(ReadOnlyMemory<byte> data)
        {
            Argument.Meets(data.Length > 0, nameof(data), "Cannot write an empty block.");

            return BaseStream.WriteBlockAsync(data);
        }
#endif

        public int BlocksWritten => BaseStream.BlocksWritten;

        public int Index => BaseStream.Index;

        public virtual string Name
        {
            get { return BaseStream.Name; }
            set { BaseStream.Name = value; }
        }

        public FourCC StreamType => BaseStream.StreamType;

        public FourCC ChunkId => BaseStream.ChunkId;

        public virtual void PrepareForWriting() => BaseStream.PrepareForWriting();

        public virtual void FinishWriting() => BaseStream.FinishWriting();

        public void WriteHeader() => BaseStream.WriteHeader();

        public void WriteFormat() => BaseStream.WriteFormat();
    }
}
