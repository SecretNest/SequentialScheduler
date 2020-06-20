using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SecretNest.TaskSchedulers
{
    /// <summary>
    /// Defines a task scheduler that run tasks on a single thread.
    /// </summary>
    public class SequentialScheduler : TaskScheduler, IDisposable
    {
        readonly BlockingCollection<Task> _taskQueue = new BlockingCollection<Task>();
        private Thread _thread;
        readonly CancellationTokenSource _cancellation;

        /// <summary>
        /// Initializes an instance of SequentialScheduler using a new free thread.
        /// </summary>
        /// <param name="waitForThread">Waiting for <see cref="Run"/> to provide thread. Default is <see langword="false"/>.</param>
        /// <remarks><p>When initializing with <paramref name="waitForThread"/> set to <see langword="false"/>, a free thread is created for this scheduler.</p>
        /// <p>When initializing with <paramref name="waitForThread"/> set to <see langword="true"/>, <see cref="Run"/> should be called from the thread which intends to be used for this scheduler.</p></remarks>
        public SequentialScheduler(bool waitForThread = false)
        {
            _cancellation = new CancellationTokenSource();
            if (!waitForThread)
            {
                _thread = new Thread(Working);
                _thread.Start();
            }
        }

        /// <summary>
        /// Provides thread for this scheduler. This method blocks the calling thread until scheduler disposed.
        /// </summary>
        public void Run()
        {
            if (_thread != null)
                throw new InvalidOperationException();
            _thread = Thread.CurrentThread;

            Working();
        }


        void Working()
        {
            while (!_disposedValue)
            {
                try
                {
                    var task = _taskQueue.Take(_cancellation.Token);
                    TryExecuteTask(task);
                }
                catch
                {
                    // ignored
                }
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return _taskQueue;
        }

        /// <inheritdoc />
        protected override void QueueTask(Task task)
        {
            _taskQueue.Add(task);
        }

        /// <inheritdoc />
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            if (Thread.CurrentThread == _thread)
            {
                return TryExecuteTask(task);
            }

            return false;
        }

        #region IDisposable Support

        private volatile bool _disposedValue;

        /// <summary>
        /// Disposes of the resources (other than memory) used by this instance.
        /// </summary>
        /// <param name="disposing">True: release both managed and unmanaged resources; False: release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _cancellation.Cancel();
                    _cancellation.Dispose();
                }

                _disposedValue = true;
            }
        }

        /// <summary>
        /// Releases all resources used by this instance.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
        }

        #endregion
    }
}