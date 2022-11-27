namespace SharpAvi.Output
{
    internal interface IAviVideoStreamInternal : IAviVideoStream, IAviStreamInternal
    {
        byte[] BitmapInfoHeader { get; set; }
    }
}
