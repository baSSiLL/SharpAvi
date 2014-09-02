using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;

namespace SharpAvi.Output
{
    internal abstract class AviStreamBase : IAviStream, IAviStreamInternal
    {
        private bool isFrozen;
        private readonly int index;
        private string name;
        private FourCC chunkId;

        protected AviStreamBase(int index)
        {
            Contract.Requires(index >= 0);

            this.index = index;
        }

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

        public abstract FourCC StreamType { get; }

        public FourCC ChunkId
        {
            get 
            { 
                if (!isFrozen)
                {
                    throw new InvalidOperationException("Chunk ID is not defined until the stream is frozen.");
                }

                return chunkId; 
            }
        }

        public abstract void WriteFormat(BinaryWriter writer);

        public void Freeze()
        {
            if (!isFrozen)
            {
                isFrozen = true;

                chunkId = GenerateChunkId();
            }
        }


        protected abstract FourCC GenerateChunkId();

        protected void CheckNotFrozen()
        {
            if (isFrozen)
            {
                throw new InvalidOperationException("No stream information can be changed after starting to write frames.");
            }
        }
    
    }
}
