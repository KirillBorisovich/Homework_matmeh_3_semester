// <copyright file="MyThreadPool.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

namespace ThreadPool;

public class MyThreadPool : IDisposable
{
    private readonly TaskQueue<Action> taskQueue = new();
    private readonly Thread[] threads;
    private readonly CancellationTokenSource cancellationTokenSource = new();
    private bool isShutdown;
    private Lock lockObject = new Lock();

    /// <summary>
    /// Initializes a new instance of the <see cref="MyThreadPool"/> class.
    /// </summary>
    public MyThreadPool()
    {
        this.threads = new Thread[Environment.ProcessorCount];
        for (var i = 0; i < this.threads.Length; i++)
        {
            this.threads[i] = new Thread(
                () => this.ProcessTheIssue(this.cancellationTokenSource.Token))
            {
                IsBackground = true,
            };
        }

        foreach (var thread in this.threads)
        {
            thread.Start();
        }
    }

    /// <summary>
    /// Gets the number of running threads.
    /// </summary>
    public int ThreadsCount => this.threads.Length;

    /// <summary>
    /// Send a task to a thread pool.
    /// </summary>
    /// <param name="func">Task to complete.</param>
    /// <typeparam name="TResult">Type of task result.</typeparam>
    /// <returns>The IMyTask interface element.</returns>
    public IMyTask<TResult> Submit<TResult>(Func<TResult> func)
    {
        var task = new MyTask<TResult>(this, func);
        this.Run(() => task.Execute());
        return task;
    }

    /// <summary>
    /// Shut down the threads.
    /// </summary>
    public void Shutdown()
    {
        this.isShutdown = true;
        this.cancellationTokenSource.Cancel();

        while (this.taskQueue.Count > 0)
        {
            var task = this.taskQueue.Dequeue();
            task();
        }

        foreach (var thread in this.threads)
        {
            thread.Join();
        }
    }

    public void Dispose()
    {
        if (!this.isShutdown)
        {
            this.Shutdown();
        }

        this.cancellationTokenSource.Dispose();
    }

    private void Run(Action action)
    {
        if (this.isShutdown)
        {
            throw new OperationCanceledException("ThreadPool is stopped");
        }

        this.taskQueue.Enqueue(action);
    }

    private void ProcessTheIssue(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var task = this.taskQueue.Dequeue();
            task();
        }
    }

    private class TaskQueue<T>
    {
        private readonly Queue<T> buffer = new();

        public int Count => this.buffer.Count;

        public void Enqueue(T item)
        {
            lock (this.buffer)
            {
                this.buffer.Enqueue(item);
                Monitor.Pulse(this.buffer);
            }
        }

        public T Dequeue()
        {
            lock (this.buffer)
            {
                while (this.buffer.Count == 0)
                {
                    Monitor.Wait(this.buffer);
                }

                return this.buffer.Dequeue();
            }
        }
    }

    private class MyTask<TResult>(MyThreadPool threadPool, Func<TResult> func) :
        IMyTask<TResult>
    {
        private readonly MyThreadPool threadPool = threadPool;
        private readonly TaskQueue<Action> continuations = new();
        private readonly ManualResetEvent completionEvent = new ManualResetEvent(false);
        private TResult? result;
        private Exception? exception;

        public bool IsCompleted { get; private set; }

        public TResult Result
        {
            get
            {
                this.completionEvent.WaitOne();
                if (this.exception != null)
                {
                    throw new AggregateException(this.exception);
                }

                return this.result!;
            }
        }

        public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> function)
        {
            if (this.exception != null)
            {
                throw new AggregateException(this.exception);
            }

            var newTask = new MyTask<TNewResult>(this.threadPool, () => function(this.Result));
            this.continuations.Enqueue(() =>
            {
                if (this.exception != null)
                {
                    newTask.exception = this.exception;
                    newTask.completionEvent.Set();
                    newTask.IsCompleted = true;
                    newTask.ExecuteAllContinuations();
                    return;
                }

                newTask.Execute();
            });

            return newTask;
        }

        public void Execute()
        {
            if (this.threadPool.isShutdown)
            {
                this.exception = new OperationCanceledException("ThreadPool is stopped");
                this.ExecuteAllContinuations();

                return;
            }

            try
            {
                this.result = func();
            }
            catch (Exception ex)
            {
                this.exception = ex;
            }

            this.completionEvent.Set();
            this.IsCompleted = true;

            if (this.exception == null)
            {
                this.SendAllContinuationsToTheThreadPool();
            }
            else
            {
                this.ExecuteAllContinuations();
            }
        }

        private void SendAllContinuationsToTheThreadPool()
        {
            while (this.continuations.Count > 0)
            {
                this.threadPool.Run(this.continuations.Dequeue());
            }
        }

        private void ExecuteAllContinuations()
        {
            while (this.continuations.Count > 0)
            {
                this.continuations.Dequeue()();
            }
        }
    }
}