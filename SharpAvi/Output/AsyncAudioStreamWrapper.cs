using System;
using System.Collections.Generic;
#if !NET35
using System.Diagnostics.Contracts;
#endif
using System.Linq;
using System.Text;
#if !NET35
using System.Threading.Tasks;
#endif

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
#if !NET35
            Contract.Requires(baseStream != null);
#endif
        }

        public override void WriteBlock(byte[] data, int startIndex, int length)
        {
            writeInvoker.Invoke(() => base.WriteBlock(data, startIndex, length));
        }

#if NET35
        public override IAsyncResult BeginWriteBlock(byte[] data, int startIndex, int length, AsyncCallback userCallback, object stateObject)
        {
            return writeInvoker.BeginInvoke(
                () => base.WriteBlock(data, startIndex, length),
                userCallback, stateObject);
        }

        public override void EndWriteBlock(IAsyncResult asyncResult)
        {
            writeInvoker.EndInvoke(asyncResult);
        }
#else
        public override Task WriteBlockAsync(byte[] data, int startIndex, int length)
        {
            return writeInvoker.InvokeAsync(() => base.WriteBlock(data, startIndex, length));
        }
#endif

        public override void FinishWriting()
        {
            // Perform all pending writes and then let the base stream to finish
            // (possibly writing some more data synchronously)
            writeInvoker.WaitForPendingInvocations();

            base.FinishWriting();
        }
    }
}
