using System.Diagnostics.Contracts;
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
            Contract.Requires(baseStream != null);
        }

        public override void WriteFrame(bool isKeyFrame, byte[] frameData, int startIndex, int length)
        {
            writeInvoker.Invoke(() => base.WriteFrame(isKeyFrame, frameData, startIndex, length));
        }

        public override Task WriteFrameAsync(bool isKeyFrame, byte[] frameData, int startIndex, int length)
        {
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
