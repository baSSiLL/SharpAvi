using System;
using System.Collections.Generic;

namespace SharpAvi.Output
{
    internal class StreamInfo
    {
        private readonly FourCC standardIndexChunkId;
        private readonly List<StandardIndexEntry> standardIndex = new List<StandardIndexEntry>();
        private readonly List<SuperIndexEntry> superIndex = new List<SuperIndexEntry>();

        public StreamInfo(FourCC standardIndexChunkId)
        {
            this.standardIndexChunkId = standardIndexChunkId;
            FrameCount = 0;
            MaxChunkDataSize = 0;
            TotalDataSize = 0;
        }

        public int FrameCount { get; private set; }
        
        public int MaxChunkDataSize { get; private set; }

        public long TotalDataSize { get; private set; }

        public IList<SuperIndexEntry> SuperIndex
        {
            get { return superIndex; }
        }
        
        public IList<StandardIndexEntry> StandardIndex 
        {
            get { return standardIndex; }
        }

        public FourCC StandardIndexChunkId
        {
            get { return standardIndexChunkId; }
        }

        public void OnFrameWritten(int chunkDataSize)
        {
            FrameCount++;
            MaxChunkDataSize = Math.Max(MaxChunkDataSize, chunkDataSize);
            TotalDataSize += chunkDataSize;
        }
    }
}
