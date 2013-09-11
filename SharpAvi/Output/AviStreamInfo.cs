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
            MaxChunkSize = 0;
        }

        public int FrameCount { get; private set; }
        
        public int MaxChunkSize { get; private set; }

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

        public void OnFrameWritten(int chunkSize)
        {
            FrameCount++;
            MaxChunkSize = Math.Max(MaxChunkSize, chunkSize);
        }
    }
}
