﻿using SharpAvi.Format;
using SharpAvi.Utilities;
using System;
using System.Threading.Tasks;

namespace SharpAvi.Output
{
    internal class AviVideoStream : AviStreamBase, IAviVideoStreamInternal
    {
        private readonly IAviStreamWriteHandler writeHandler;
        private FourCC streamCodec;
        private int width;
        private int height;
        private BitsPerPixel bitsPerPixel;
        private int framesWritten;
        private byte[] bitmapInfoHeader;

        public AviVideoStream(int index, IAviStreamWriteHandler writeHandler, 
            int width, int height, BitsPerPixel bitsPerPixel)
            : base(index)
        {
            Argument.IsNotNull(writeHandler, nameof(writeHandler));
            Argument.IsPositive(width, nameof(width));
            Argument.IsPositive(height, nameof(height));
            Argument.IsEnumMember(bitsPerPixel, nameof(bitsPerPixel));

            this.writeHandler = writeHandler;
            this.width = width;
            this.height = height;
            this.bitsPerPixel = bitsPerPixel;
            this.streamCodec = CodecIds.Uncompressed;
        }


        public int Width
        {
            get { return width; }
            set
            {
                CheckNotFrozen();
                width = value;
            }
        }

        public int Height
        {
            get { return height; }
            set
            {
                CheckNotFrozen();
                height = value;
            }
        }

        public BitsPerPixel BitsPerPixel
        {
            get { return bitsPerPixel; }
            set
            {
                CheckNotFrozen();
                bitsPerPixel = value;
            }
        }

        public FourCC Codec
        {
            get { return streamCodec; }
            set
            {
                CheckNotFrozen();
                streamCodec = value;
            }
        }

        public byte[] BitmapInfoHeader
        {
            get { return bitmapInfoHeader; }
            set
            {
                CheckNotFrozen();
                bitmapInfoHeader = value;
            }
        }

        public void WriteFrame(bool isKeyFrame, byte[] frameData, int startIndex, int count)
        {
            Argument.IsNotNull(frameData, nameof(frameData));
            Argument.IsNotNegative(startIndex, nameof(startIndex));
            Argument.IsPositive(count, nameof(count));
            Argument.ConditionIsMet(startIndex + count <= frameData.Length, "End offset exceeds the length of frame data.");

#if NET5_0_OR_GREATER
            WriteFrame(isKeyFrame, frameData.AsSpan(startIndex, count));
#else
            writeHandler.WriteVideoFrame(this, isKeyFrame, frameData, startIndex, count);
            System.Threading.Interlocked.Increment(ref framesWritten);
#endif
        }

        public Task WriteFrameAsync(bool isKeyFrame, byte[] frameData, int startIndex, int count) 
            => throw new NotSupportedException("Asynchronous writes are not supported.");

#if NET5_0_OR_GREATER
        public void WriteFrame(bool isKeyFrame, ReadOnlySpan<byte> frameData)
        {
            Argument.Meets(frameData.Length > 0, nameof(frameData), "Cannot write an empty frame.");

            writeHandler.WriteVideoFrame(this, isKeyFrame, frameData);
            System.Threading.Interlocked.Increment(ref framesWritten);
        }

        public Task WriteFrameAsync(bool isKeyFrame, ReadOnlyMemory<byte> frameData)
            => throw new NotSupportedException("Asynchronous writes are not supported.");
#endif

        public int FramesWritten => framesWritten;


        public override FourCC StreamType => KnownFourCCs.StreamTypes.Video;

        protected override FourCC GenerateChunkId() 
            => KnownFourCCs.Chunks.VideoFrame(Index, Codec != CodecIds.Uncompressed);

        public override void WriteHeader() => writeHandler.WriteStreamHeader(this);

        public override void WriteFormat() => writeHandler.WriteStreamFormat(this);
    }
}
