using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace SharpAvi.Output
{
    /// <summary>
    /// Adds asynchronous writes support for underlying stream.
    /// </summary>
    internal class AsyncAudioStreamWrapper : AudioStreamWrapperBase
    {
        private readonly SequentialInvoker writeInvoker = new SequentialInvoker();

        public AsyncAudioStreamWrapper(IAviAudioStreamInternal baseStream)
            : base(baseStream)
        {
            Contract.Requires(baseStream != null);
        }

        public override void WriteBlock(byte[] data, int startIndex, int length)
        {
            writeInvoker.Invoke(() => base.WriteBlock(data, startIndex, length));
        }

        public override Task WriteBlockAsync(byte[] data, int startIndex, int length)
        {
            return writeInvoker.InvokeAsync(() => base.WriteBlock(data, startIndex, length));
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
