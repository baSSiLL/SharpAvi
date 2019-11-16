
#if NET35

using System;
using System.Collections.Generic;
#if !NET35
using System.Diagnostics.Contracts;
#endif
using System.Linq;
using System.Text;
using System.Threading;

namespace SharpAvi.Output
{
    /// <summary>
    /// Serializes synchronous and asynchronous invocations in one queue.
    /// </summary>
    internal sealed class SequentialInvoker
    {
        private WaitHandle lastEvent = new ManualResetEvent(true);
        private readonly object syncLastEvent = new object();

        public SequentialInvoker()
        {
        }

        public void Invoke(Action action)
        {
            WaitHandle prevEvent;
            var newEvent = new ManualResetEvent(false);
            lock (syncLastEvent)
            {
                prevEvent = lastEvent;
                lastEvent = newEvent;
            }

            try
            {
                SafeWait(prevEvent);
                action.Invoke();
            }
            finally
            {
                newEvent.Set();
            }
        }

        public IAsyncResult BeginInvoke(Action action, AsyncCallback userCallback, object stateObject)
        {
            var asyncInfo = new AsyncInvocationInfo(this, action, userCallback, stateObject);
            lock (syncLastEvent)
            {
                asyncInfo.PrevWaitHandle = lastEvent;
                lastEvent = asyncInfo.AsyncWaitHandle;
            }
            ThreadPool.QueueUserWorkItem(InvokeAsync, asyncInfo);
            return asyncInfo;
        }

        public void EndInvoke(IAsyncResult asyncResult)
        {
#if !NET35
            Contract.Requires(asyncResult != null);
#endif

            var asyncInfo = asyncResult as AsyncInvocationInfo;
            if (asyncInfo == null || asyncInfo.Owner != this)
            {
                throw new ArgumentOutOfRangeException("asyncResult");
            }

            asyncInfo.End();

            if (asyncInfo.Exception != null)
            {
                throw asyncInfo.Exception;
            }
        }

        public void WaitForPendingInvocations()
        {
            WaitHandle lastEvent;
            lock (syncLastEvent)
            {
                lastEvent = this.lastEvent;
            }
            SafeWait(lastEvent);
        }


        private static void InvokeAsync(object state)
        {
            var asyncInfo = (AsyncInvocationInfo)state;

            try
            {
                SafeWait(asyncInfo.PrevWaitHandle);
                asyncInfo.Action.Invoke();
                asyncInfo.SetResult(null);
            }
            catch (Exception ex)
            {
                asyncInfo.SetResult(ex);
            }

            if (asyncInfo.UserCallback != null)
            {
                asyncInfo.UserCallback.Invoke(asyncInfo);
            }
        }

        private static void SafeWait(WaitHandle waitHandle)
        {
            try
            {
                waitHandle.WaitOne();
            }
            catch (ObjectDisposedException)
            {
            }
        }


        private class AsyncInvocationInfo : IAsyncResult
        {
            private bool isEnded;

            public AsyncInvocationInfo(SequentialInvoker owner, Action action, AsyncCallback userCallback, object stateObject)
            {
#if !NET35
                Contract.Requires(owner != null);
                Contract.Requires(action != null);
#endif

                this.owner = owner;
                this.action = action;
                this.userCallback = userCallback;
                this.asyncState = stateObject;
            }

            public SequentialInvoker Owner
            {
                get { return owner; }
            }

            private readonly SequentialInvoker owner;

            public Action Action
            {
                get { return action; }
            }
            private readonly Action action;

            public AsyncCallback UserCallback
            {
                get { return userCallback; }
            }
            private readonly AsyncCallback userCallback;

            public object AsyncState
            {
                get { return asyncState; }
            }

            private readonly object asyncState;

            public WaitHandle AsyncWaitHandle
            {
                get { return completedEvent; }
            }

            private readonly ManualResetEvent completedEvent = new ManualResetEvent(false);

            public bool CompletedSynchronously
            {
                get { return false; }
            }

#if !NET35
            [Pure]
#endif
            public bool IsCompleted
            {
                get { return isEnded || completedEvent.WaitOne(0); }
            }

            public WaitHandle PrevWaitHandle
            {
                get;
                set;
            }

            public Exception Exception
            {
                get { lock (syncException) return exception; }
                private set { lock (syncException) exception = value; }
            }

            private Exception exception;
            private readonly object syncException = new object();

            public void SetResult(Exception ex)
            {
                if (isEnded || completedEvent.WaitOne(0))
                {
                    throw new InvalidOperationException();
                }

                Exception = ex;
                completedEvent.Set();
            }

            public void End()
            {
                if (isEnded)
                {
                    throw new InvalidOperationException("End is called multiple times on the same object.");
                }

                completedEvent.WaitOne();
                completedEvent.Close();
                isEnded = true;
            }
        }
    }
}

#else

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

#endif
