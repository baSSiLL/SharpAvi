using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
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
    }
}
