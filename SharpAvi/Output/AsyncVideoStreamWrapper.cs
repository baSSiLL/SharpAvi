using System.Threading.Tasks;

namespace SharpAvi.Output
{
    /// <summary>
    /// Adds asynchronous writes support for underlying stream.
    /// </summary>
    internal class AsyncVideoStreamWrapper : VideoStreamWrapperBase
    {
        private readonly SequentialInvoker writeInvoker = new SequentialInvoker();

        public AsyncVideoStreamWrapper(IAviVideoStreamInternal baseStream)
            : base(baseStream)
        {
        }

        public override void WriteFrame(bool isKeyFrame, byte[] frameData, int startIndex, int length)
        {
            Argument.IsNotNull(frameData, nameof(frameData));
            Argument.IsNotNegative(startIndex, nameof(startIndex));
            Argument.IsPositive(length, nameof(length));
            Argument.ConditionIsMet(startIndex + length <= frameData.Length, "End offset exceeds the length of frame data.");

            writeInvoker.Invoke(() => base.WriteFrame(isKeyFrame, frameData, startIndex, length));
        }

        public override Task WriteFrameAsync(bool isKeyFrame, byte[] frameData, int startIndex, int length)
        {
            Argument.IsNotNull(frameData, nameof(frameData));
            Argument.IsNotNegative(startIndex, nameof(startIndex));
            Argument.IsPositive(length, nameof(length));
            Argument.ConditionIsMet(startIndex + length <= frameData.Length, "End offset exceeds the length of frame data.");

            return writeInvoker.InvokeAsync(() => base.WriteFrame(isKeyFrame, frameData, startIndex, length));
        }

        public override void FinishWriting()
        {
            // Perform all pending writes and then let the base stream to finish
            // (possibly writing some more data synchronously)
            writeInvoker.WaitForPendingInvocations();

            base.FinishWriting();
        }
    }
}
