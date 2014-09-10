using System.IO;

namespace SharpAvi.Output
{
    internal interface IAviStreamInternal : IAviStream
    {
        FourCC StreamType { get; }
        FourCC ChunkId { get; }
        void Freeze();
        void WriteHeader();
        void WriteFormat();
    }
}
