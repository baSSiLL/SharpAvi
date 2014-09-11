using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
#if FX45
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
            Contract.Requires(baseStream != null);
        }

        public override void WriteBlock(byte[] data, int startIndex, int length)
        {
            writeInvoker.Invoke(() => base.WriteBlock(data, startIndex, length));
        }

#if FX45
        public override Task WriteBlockAsync(byte[] data, int startIndex, int length)
        {
            return writeInvoker.InvokeAsync(() => base.WriteBlock(data, startIndex, length));
        }
#else
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
