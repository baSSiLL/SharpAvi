#if !NET35
using System.Diagnostics.Contracts;
#endif

namespace SharpAvi.Output
{
    /// <summary>
    /// Item of a RIFF file - either list or chunk.
    /// </summary>
    internal struct RiffItem
    {
        public const int ITEM_HEADER_SIZE = 2 * sizeof(uint);

        private readonly long dataStart;
        private int dataSize;

        public RiffItem(long dataStart, int dataSize = -1)
        {
#if !NET35
            Contract.Requires(dataStart >= ITEM_HEADER_SIZE);
            Contract.Requires(dataSize <= int.MaxValue - ITEM_HEADER_SIZE);
#endif

            this.dataStart = dataStart;
            this.dataSize = dataSize;
        }

        public long DataStart
        {
            get { return dataStart; }
        }

        public long ItemStart
        {
            get { return dataStart - ITEM_HEADER_SIZE; }
        }

        public long DataSizeStart
        {
            get { return dataStart - sizeof(uint); }
        }

        public int DataSize
        {
            get { return dataSize; }
            set
            {
#if !NET35
                Contract.Requires(value >= 0);
                Contract.Requires(DataSize < 0);
#endif

                dataSize = value;
            }
        }

        public int ItemSize
        {
            get { return dataSize < 0 ? -1 : dataSize + ITEM_HEADER_SIZE; }
        }
    }
}
