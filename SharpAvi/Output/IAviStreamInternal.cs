using System.IO;

namespace SharpAvi.Output
{
    internal interface IAviStreamInternal
    {
        FourCC StreamType { get; }
        FourCC ChunkId { get; }
        void Freeze();
        void WriteFormat(BinaryWriter writer);
    }
}
