using System;
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
        }

        public override void WriteBlock(byte[] data, int startIndex, int length)
        {
            Argument.IsNotNull(data, nameof(data));
            Argument.IsNotNegative(startIndex, nameof(startIndex));
            Argument.IsPositive(length, nameof(length));
            Argument.ConditionIsMet(startIndex + length <= data.Length, "End offset exceeds the length of data.");

            writeInvoker.Invoke(() => base.WriteBlock(data, startIndex, length));
        }

        public override Task WriteBlockAsync(byte[] data, int startIndex, int length)
        {
            Argument.IsNotNull(data, nameof(data));
            Argument.IsNotNegative(startIndex, nameof(startIndex));
            Argument.IsPositive(length, nameof(length));
            Argument.ConditionIsMet(startIndex + length <= data.Length, "End offset exceeds the length of data.");

            return writeInvoker.InvokeAsync(() => base.WriteBlock(data, startIndex, length));
        }

#if NET5_0_OR_GREATER
        public override void WriteBlock(ReadOnlySpan<byte> data)
        {
            Argument.Meets(data.Length > 0, nameof(data), "Cannot write an empty block.");

#warning Implement WriteBlock
            throw new NotImplementedException();
            //writeInvoker.Invoke(() => base.WriteBlock(data));
        }

        public override Task WriteBlockAsync(ReadOnlyMemory<byte> data)
        {
            Argument.Meets(data.Length > 0, nameof(data), "Cannot write an empty block.");

            return writeInvoker.InvokeAsync(() => base.WriteBlock(data.Span));
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
