using SharpAvi.Utilities;
using System;
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
            Argument.IsNotNull(baseStream, nameof(baseStream));

            this.BaseStream = baseStream;
        }

        protected IAviVideoStreamInternal BaseStream { get; }

        public virtual void Dispose() => (BaseStream as IDisposable)?.Dispose();

        public virtual int Width
        {
            get { return BaseStream.Width; }
            set { BaseStream.Width = value; }
        }

        public virtual int Height
        {
            get { return BaseStream.Height; }
            set { BaseStream.Height = value; }
        }

        public virtual BitsPerPixel BitsPerPixel
        {
            get { return BaseStream.BitsPerPixel; }
            set { BaseStream.BitsPerPixel = value; }
        }

        public virtual FourCC Codec
        {
            get { return BaseStream.Codec; }
            set { BaseStream.Codec = value; }
        }

        public virtual byte[] BitmapInfoHeader
        {
            get { return BaseStream.BitmapInfoHeader; }
            set { BaseStream.BitmapInfoHeader = value; }
        }

        public virtual void WriteFrame(bool isKeyFrame, byte[] frameData, int startIndex, int length)
        {
            Argument.IsNotNull(frameData, nameof(frameData));
            Argument.IsNotNegative(startIndex, nameof(startIndex));
            Argument.IsPositive(length, nameof(length));
            Argument.ConditionIsMet(startIndex + length <= frameData.Length, "End offset exceeds the length of frame data.");

            BaseStream.WriteFrame(isKeyFrame, frameData, startIndex, length);
        }

        public virtual Task WriteFrameAsync(bool isKeyFrame, byte[] frameData, int startIndex, int length)
        {
            Argument.IsNotNull(frameData, nameof(frameData));
            Argument.IsNotNegative(startIndex, nameof(startIndex));
            Argument.IsPositive(length, nameof(length));
            Argument.ConditionIsMet(startIndex + length <= frameData.Length, "End offset exceeds the length of frame data.");

            return BaseStream.WriteFrameAsync(isKeyFrame, frameData, startIndex, length);
        }

#if NET5_0_OR_GREATER
        public virtual void WriteFrame(bool isKeyFrame, ReadOnlySpan<byte> frameData)
        {
            Argument.Meets(frameData.Length > 0, nameof(frameData), "Cannot write an empty frame.");

            BaseStream.WriteFrame(isKeyFrame, frameData);
        }

        public virtual Task WriteFrameAsync(bool isKeyFrame, ReadOnlyMemory<byte> frameData)
        {
            Argument.Meets(frameData.Length > 0, nameof(frameData), "Cannot write an empty frame.");

            return BaseStream.WriteFrameAsync(isKeyFrame, frameData);
        }
#endif

        public int FramesWritten => BaseStream.FramesWritten;

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
