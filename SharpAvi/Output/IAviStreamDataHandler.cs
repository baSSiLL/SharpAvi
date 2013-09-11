namespace SharpAvi.Output
{
    internal interface IAviStreamDataHandler
    {
        void WriteVideoFrame(AviVideoStream stream, bool isKeyFrame, byte[] frameData, int startIndex, int count);
    }
}
