using System;
using System.Diagnostics.Contracts;

namespace SharpAvi.Output
{
    internal class AviVideoStream : AviStreamBase, IAviVideoStream
    {
        private readonly IAviStreamWriteHandler writeHandler;
        private FourCC streamCodec = KnownFourCCs.Codecs.Uncompressed;
        private int width;
        private int height;
        private BitsPerPixel bitsPerPixel = BitsPerPixel.Bpp24;

        public AviVideoStream(int index, IAviStreamWriteHandler writeHandler)
            : base(index)
        {
            Contract.Requires(index >= 0);
            Contract.Requires(writeHandler != null);

            this.writeHandler = writeHandler;
            FramesWritten = 0;
        }


        #region IAviVideoStream implementation

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

        public void WriteFrame(bool isKeyFrame, byte[] frameData, int startIndex, int count)
        {
            writeHandler.WriteVideoFrame(this, isKeyFrame, frameData, startIndex, count);
            FramesWritten++;
        }

        public int FramesWritten
        {
            get;
            private set;
        }
        
        #endregion


        public override FourCC StreamType
        {
            get { return KnownFourCCs.StreamTypes.Video; }
        }

        protected override FourCC GenerateChunkId()
        {
            return KnownFourCCs.Chunks.VideoFrame(Index, Codec != KnownFourCCs.Codecs.Uncompressed);
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
