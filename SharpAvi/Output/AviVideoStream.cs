using System;
using System.Diagnostics.Contracts;

namespace SharpAvi.Output
{
    internal class AviVideoStream : AviStreamBase, IAviVideoStream
    {
        private readonly IAviStreamDataHandler dataHandler;
        private FourCC streamCodec = KnownFourCCs.Codecs.Uncompressed;
        private int width;
        private int height;
        private BitsPerPixel bitsPerPixel = BitsPerPixel.Bpp24;

        public AviVideoStream(int index, IAviStreamDataHandler dataHandler)
            : base(index)
        {
            Contract.Requires(index >= 0);
            Contract.Requires(dataHandler != null);

            this.dataHandler = dataHandler;
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
            dataHandler.WriteVideoFrame(this, isKeyFrame, frameData, startIndex, count);
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

        public override void WriteFormat(System.IO.BinaryWriter writer)
        {
            // See BITMAPINFOHEADER structure
            writer.Write(40U); // size of structure
            writer.Write(Width);
            writer.Write(Height);
            writer.Write((short)1); // planes
            writer.Write((ushort)BitsPerPixel); // bits per pixel
            writer.Write((uint)Codec); // compression (codec FOURCC)
            writer.Write((uint)(Width * Height * (((int)BitsPerPixel) / 8))); // image size in bytes
            writer.Write(0); // X pixels per meter
            writer.Write(0); // Y pixels per meter
            writer.Write(0U); // palette colors used
            writer.Write(0U); // palette colors important
        }
    }
}
