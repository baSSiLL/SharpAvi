using System;
using System.Diagnostics.Contracts;

namespace SharpAvi.Output
{
    internal class AviVideoStream : IAviVideoStream, IAviStreamInternal
    {
        private bool isFrozen;
        private readonly int index;
        private readonly IAviStreamDataHandler dataHandler;
        private string name;
        private FourCC streamCodec = KnownFourCCs.Codecs.Uncompressed;
        private int width;
        private int height;
        private BitsPerPixel bitsPerPixel = BitsPerPixel.Bpp24;
        private FourCC chunkId;

        public AviVideoStream(int index, IAviStreamDataHandler dataHandler)
        {
            Contract.Requires(index >= 0);
            Contract.Requires(dataHandler != null);

            this.index = index;
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

        public void WriteFrame(bool isKeyFrame, byte[] frameData, int startIndex, int count)
        {
            dataHandler.WriteVideoFrame(this, isKeyFrame, frameData, startIndex, count);
        }
        
        #endregion


        #region IAviStream implementation

        public int Index
        {
            get { return index; }
        }

        public string Name
        {
            get { return name; }
            set
            {
                CheckNotFrozen();
                name = value;
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

        #endregion


        #region IAviStreamInternal implementation

        public FourCC StreamType
        {
            get { return KnownFourCCs.StreamTypes.Video; }
        }

        public FourCC ChunkId
        {
            get { return chunkId; }
        }

        public void Freeze()
        {
            if (!isFrozen)
            {
                isFrozen = true;

                chunkId = KnownFourCCs.Chunks.VideoFrame(Index, Codec != KnownFourCCs.Codecs.Uncompressed);
            }
        }

        public void WriteFormat(System.IO.BinaryWriter writer)
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

        #endregion


        private void CheckNotFrozen()
        {
            if (isFrozen)
            {
                throw new InvalidOperationException("No stream information can be changed after starting to write frames.");
            }
        }
    }
}
