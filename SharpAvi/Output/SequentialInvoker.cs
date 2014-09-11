using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace SharpAvi.Output
{
    /// <summary>
    /// Serializes synchronous and asynchronous invocations in one queue.
    /// </summary>
    internal sealed class SequentialInvoker
    {
        private readonly object sync = new object();
        private Task lastTask;

        /// <summary>
        /// Creates a new instance of <see cref="SequentialInvoker"/>.
        /// </summary>
        public SequentialInvoker()
        {
            var tcs = new TaskCompletionSource<bool>();
            tcs.SetResult(true);

            // Initialize lastTask to already completed task
            lastTask = tcs.Task;
        }

        /// <summary>
        /// Invokes an action synchronously.
        /// </summary>
        /// <param name="action">Action.</param>
        /// <remarks>
        /// Waits for any previously scheduled invocations to complete.
        /// </remarks>
        public void Invoke(Action action)
        {
            Contract.Requires(action != null);

            Task prevTask;
            var tcs = new TaskCompletionSource<bool>();

            lock (sync)
            {
                prevTask = lastTask;
                lastTask = tcs.Task;
            }

            try
            {
                prevTask.Wait();
                try
                {
                    action.Invoke();
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                    throw;
                }
                tcs.SetResult(true);
            }
            finally
            {
                tcs.TrySetResult(false);
            }
        }

        /// <summary>
        /// Schedules an action asynchronously.
        /// </summary>
        /// <param name="action">Action.</param>
        /// <returns>Task corresponding to asunchronous invocation.</returns>
        /// <remarks>
        /// This action will be invoked after all previously scheduled invocations complete.
        /// </remarks>
        public Task InvokeAsync(Action action)
        {
            Contract.Requires(action != null);

            Task result;
            lock (sync)
            {
                result = lastTask.ContinueWith(_ => action.Invoke());
                lastTask = result;
            }

            return result;
        }

        /// <summary>
        /// Waits for currently pending invocations to complete.
        /// </summary>
        /// <remarks>
        /// New invocations, which are possibly scheduled during this call, are not considered.
        /// </remarks>
        public void WaitForPendingInvocations()
        {
            Task taskToWait;
            lock (sync)
            {
                taskToWait = lastTask;
            }
            taskToWait.Wait();
        }
    }
}
