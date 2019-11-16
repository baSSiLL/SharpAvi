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
    internal class AsyncVideoStreamWrapper : VideoStreamWrapperBase
    {
        private readonly SequentialInvoker writeInvoker = new SequentialInvoker();

        public AsyncVideoStreamWrapper(IAviVideoStreamInternal baseStream)
            : base(baseStream)
        {
#if !NET35
            Contract.Requires(baseStream != null);
#endif
        }

        public override void WriteFrame(bool isKeyFrame, byte[] frameData, int startIndex, int length)
        {
            writeInvoker.Invoke(() => base.WriteFrame(isKeyFrame, frameData, startIndex, length));
        }

#if NET35
        public override IAsyncResult BeginWriteFrame(bool isKeyFrame, byte[] frameData, int startIndex, int length, AsyncCallback userCallback, object stateObject)
        {
            return writeInvoker.BeginInvoke(
                () => base.WriteFrame(isKeyFrame, frameData, startIndex, length),
                userCallback, stateObject);
        }

        public override void EndWriteFrame(IAsyncResult asyncResult)
        {
            writeInvoker.EndInvoke(asyncResult);
        }
#else
        public override Task WriteFrameAsync(bool isKeyFrame, byte[] frameData, int startIndex, int length)
        {
            return writeInvoker.InvokeAsync(() => base.WriteFrame(isKeyFrame, frameData, startIndex, length));
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
