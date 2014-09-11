namespace SharpAvi.Output
{
    /// <summary>
    /// Interface of an object performing actual writing for the streams.
    /// </summary>
    internal interface IAviStreamWriteHandler
    {
        void WriteVideoFrame(AviVideoStream stream, bool isKeyFrame, byte[] frameData, int startIndex, int count);
        void WriteAudioBlock(AviAudioStream stream, byte[] blockData, int startIndex, int count);

        void WriteStreamHeader(AviVideoStream stream);
        void WriteStreamHeader(AviAudioStream stream);

        void WriteStreamFormat(AviVideoStream stream);
        void WriteStreamFormat(AviAudioStream stream);
    }
}
