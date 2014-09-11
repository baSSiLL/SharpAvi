using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading;

namespace SharpAvi.Output
{
    /// <summary>
    /// Serializes synchronous and asynchronous invocations in one queue.
    /// </summary>
    internal class SequentialInvoker
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
            Contract.Requires(asyncResult != null);

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
                Contract.Requires(owner != null);
                Contract.Requires(action != null);

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

            [Pure]
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
