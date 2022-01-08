using System;

namespace SharpAvi.Output
{
    /// <summary>
    /// Interface of an object performing actual writing for the streams.
    /// </summary>
    internal interface IAviStreamWriteHandler
    {
#if NET5_0_OR_GREATER
        void WriteVideoFrame(AviVideoStream stream, bool isKeyFrame, ReadOnlySpan<byte> frameData);
        void WriteAudioBlock(AviAudioStream stream, ReadOnlySpan<byte> blockData);
#else
        void WriteVideoFrame(AviVideoStream stream, bool isKeyFrame, byte[] frameData, int startIndex, int count);
        void WriteAudioBlock(AviAudioStream stream, byte[] blockData, int startIndex, int count);
#endif

        void WriteStreamHeader(AviVideoStream stream);
        void WriteStreamHeader(AviAudioStream stream);

        void WriteStreamFormat(AviVideoStream stream);
        void WriteStreamFormat(AviAudioStream stream);
    }
}
