using SharpAvi.Utilities;
using System;

namespace SharpAvi.Output
{
    /// <summary>
    /// Item of a RIFF file - either list or chunk.
    /// </summary>
    internal struct RiffItem
    {
        public const int ITEM_HEADER_SIZE = 2 * sizeof(uint);
        private int dataSize;

        public RiffItem(long dataStart, int dataSize = -1)
        {
            Argument.Meets(dataStart >= ITEM_HEADER_SIZE, nameof(dataStart));
            Argument.Meets(dataSize <= int.MaxValue - ITEM_HEADER_SIZE, nameof(dataSize));

            this.DataStart = dataStart;
            this.dataSize = dataSize;
        }

        public long DataStart { get; }

        public long ItemStart => DataStart - ITEM_HEADER_SIZE;

        public long DataSizeStart => DataStart - sizeof(uint);

        public int DataSize
        {
            get { return dataSize; }
            set
            {
                Argument.IsNotNegative(value, nameof(value));
                
                if (DataSize >= 0)
                    throw new InvalidOperationException("Data size has been already set.");

                dataSize = value;
            }
        }

        public int ItemSize => dataSize < 0 ? -1 : dataSize + ITEM_HEADER_SIZE;
    }
}
