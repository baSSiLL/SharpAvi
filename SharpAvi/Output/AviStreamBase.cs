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

        public abstract void WriteHeader();

        public abstract void WriteFormat();

        /// <summary>
        /// Prepares the stream for writing.
        /// </summary>
        /// <remarks>
        /// Default implementation freezes properties of the stream (further modifications are not allowed).
        /// </remarks>
        public virtual void PrepareForWriting()
        {
            if (!isFrozen)
            {
                isFrozen = true;

                chunkId = GenerateChunkId();
            }
        }

        /// <summary>
        /// Performs actions before closing the stream.
        /// </summary>
        /// <remarks>
        /// Default implementation does nothing.
        /// </remarks>
        public virtual void FinishWriting()
        {
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
