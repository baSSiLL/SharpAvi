using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
#if FX45
using System.Threading.Tasks;
#endif

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
            Contract.Requires(baseStream != null);

            this.baseStream = baseStream;
        }

        protected IAviAudioStreamInternal BaseStream
        {
            get { return baseStream; }
        }
        private readonly IAviAudioStreamInternal baseStream;

        public virtual void Dispose()
        {
            var baseStreamDisposable = baseStream as IDisposable;
            if (baseStreamDisposable != null)
            {
                baseStreamDisposable.Dispose();
            }
        }

        public virtual int ChannelCount
        {
            get { return baseStream.ChannelCount; }
            set { baseStream.ChannelCount = value; }
        }

        public virtual int SamplesPerSecond
        {
            get { return baseStream.SamplesPerSecond; }
            set { baseStream.SamplesPerSecond = value; }
        }

        public virtual int BitsPerSample
        {
            get { return baseStream.BitsPerSample; }
            set { baseStream.BitsPerSample = value; }
        }

        public virtual short Format
        {
            get { return baseStream.Format; }
            set { baseStream.Format = value; }
        }

        public virtual int BytesPerSecond
        {
            get { return baseStream.BytesPerSecond; }
            set { baseStream.BytesPerSecond = value; }
        }

        public virtual int Granularity
        {
            get { return baseStream.Granularity; }
            set { baseStream.Granularity = value; }
        }

        public virtual byte[] FormatSpecificData
        {
            get { return baseStream.FormatSpecificData; }
            set { baseStream.FormatSpecificData = value; }
        }

        public virtual void WriteBlock(byte[] data, int startIndex, int length)
        {
            baseStream.WriteBlock(data, startIndex, length);
        }

#if FX45
        public virtual Task WriteBlockAsync(byte[] data, int startIndex, int length)
        {
            return baseStream.WriteBlockAsync(data, startIndex, length);
        }
#else
        public virtual IAsyncResult BeginWriteBlock(byte[] data, int startIndex, int length, AsyncCallback userCallback, object stateObject)
        {
            return baseStream.BeginWriteBlock(data, startIndex, length, userCallback, stateObject);
        }

        public virtual void EndWriteBlock(IAsyncResult asyncResult)
        {
            baseStream.EndWriteBlock(asyncResult);
        }
#endif

        public int BlocksWritten
        {
            get { return baseStream.BlocksWritten; }
        }

        public int Index
        {
            get { return baseStream.Index; }
        }

        public virtual string Name
        {
            get { return baseStream.Name; }
            set { baseStream.Name = value; }
        }

        public FourCC StreamType
        {
            get { return baseStream.StreamType; }
        }

        public FourCC ChunkId
        {
            get { return baseStream.ChunkId; }
        }

        public virtual void PrepareForWriting()
        {
            baseStream.PrepareForWriting();
        }

        public virtual void FinishWriting()
        {
            baseStream.FinishWriting();
        }

        public void WriteHeader()
        {
            baseStream.WriteHeader();
        }

        public void WriteFormat()
        {
            baseStream.WriteFormat();
        }
    }
}
