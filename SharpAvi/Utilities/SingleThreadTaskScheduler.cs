using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SharpAvi.Utilities
{
    internal sealed class SingleThreadTaskScheduler : TaskScheduler, IDisposable
    {
        private readonly BlockingCollection<Task> tasks = new BlockingCollection<Task>();
        private readonly Thread thread;

        public SingleThreadTaskScheduler()
        {
            thread = new Thread(RunTasks)
            {
                IsBackground = true,
                Name = nameof(SingleThreadTaskScheduler)
            };
            thread.Start();
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                tasks.CompleteAdding();
                if (thread.ThreadState != ThreadState.Unstarted)
                {
                    thread.Join();
                }
                tasks.Dispose();
                IsDisposed = true;
            }
        }

        public bool IsDisposed { get; private set; }

        public override int MaximumConcurrencyLevel => 1;

        protected override void QueueTask(Task task) => tasks.Add(task);

        protected override IEnumerable<Task> GetScheduledTasks() => tasks.ToArray();

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) => false;

        protected override bool TryDequeue(Task task) => false;

        private void RunTasks()
        {
            foreach (var task in tasks.GetConsumingEnumerable())
            {
                TryExecuteTask(task);
            }
        }
    }
}
