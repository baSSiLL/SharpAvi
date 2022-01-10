using SharpAvi.Format;
using SharpAvi.Utilities;
using System;
using System.Collections.Generic;

namespace SharpAvi.Output
{
    internal class StreamInfo
    {
        public StreamInfo(FourCC standardIndexChunkId)
        {
            this.StandardIndexChunkId = standardIndexChunkId;
            FrameCount = 0;
            MaxChunkDataSize = 0;
            TotalDataSize = 0;
        }

        public int FrameCount { get; private set; }
        
        public int MaxChunkDataSize { get; private set; }

        public long TotalDataSize { get; private set; }

        public IList<SuperIndexEntry> SuperIndex { get; } = new List<SuperIndexEntry>();

        public IList<StandardIndexEntry> StandardIndex { get; } = new List<StandardIndexEntry>();

        public IList<Index1Entry> Index1 { get; } = new List<Index1Entry>();

        public FourCC StandardIndexChunkId { get; }

        public void OnFrameWritten(int chunkDataSize)
        {
            Argument.IsNotNegative(chunkDataSize, nameof(chunkDataSize));

            FrameCount++;
            MaxChunkDataSize = Math.Max(MaxChunkDataSize, chunkDataSize);
            TotalDataSize += chunkDataSize;
        }
    }
}
