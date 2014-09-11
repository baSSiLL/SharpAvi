using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAvi.Output
{
    /// <summary>
    /// Base class for wrappers around <see cref="IAviVideoStreamInternal"/>.
    /// </summary>
    /// <remarks>
    /// Simply delegates all operations to wrapped stream.
    /// </remarks>
    internal abstract class VideoStreamWrapperBase : IAviVideoStreamInternal, IDisposable
    {
        protected VideoStreamWrapperBase(IAviVideoStreamInternal baseStream)
        {
            Contract.Requires(baseStream != null);

            this.baseStream = baseStream;
        }

        protected IAviVideoStreamInternal BaseStream
        {
            get { return baseStream; }
        }
        private readonly IAviVideoStreamInternal baseStream;

        public virtual void Dispose()
        {
            var baseStreamDisposable = baseStream as IDisposable;
            if (baseStreamDisposable != null)
            {
                baseStreamDisposable.Dispose();
            }
        }

        public virtual int Width
        {
            get { return baseStream.Width; }
            set { baseStream.Width = value; }
        }

        public virtual int Height
        {
            get { return baseStream.Height; }
            set { baseStream.Height = value; }
        }

        public virtual BitsPerPixel BitsPerPixel
        {
            get { return baseStream.BitsPerPixel; }
            set { baseStream.BitsPerPixel = value; }
        }

        public virtual FourCC Codec
        {
            get { return baseStream.Codec; }
            set { baseStream.Codec = value; }
        }

        public virtual void WriteFrame(bool isKeyFrame, byte[] frameData, int startIndex, int length)
        {
            baseStream.WriteFrame(isKeyFrame, frameData, startIndex, length);
        }

        public virtual Task WriteFrameAsync(bool isKeyFrame, byte[] frameData, int startIndex, int length)
        {
            return baseStream.WriteFrameAsync(isKeyFrame, frameData, startIndex, length);
        }

        public int FramesWritten
        {
            get { return baseStream.FramesWritten; }
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

        public virtual void Freeze()
        {
            baseStream.Freeze();
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
