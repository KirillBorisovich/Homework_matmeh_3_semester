// <copyright file="MyThreadPool.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ThreadPool;

public class MyThreadPool : IDisposable
{
    private readonly TaskQueue<Action> taskQueue;
    private readonly Thread[] threads;
    private readonly CancellationTokenSource cancellationTokenSource = new();
    private bool isShutdown;

    /// <summary>
    /// Initializes a new instance of the <see cref="MyThreadPool"/> class.
    /// </summary>
    public MyThreadPool()
    {
        this.taskQueue = new TaskQueue<Action>(this);
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
        if (this.isShutdown)
        {
            throw new OperationCanceledException("ThreadPool is stopped");
        }

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

        this.taskQueue.WakeAll();
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
            try
            {
                var task = this.taskQueue.Dequeue();
                task();
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }

    private class TaskQueue<T>(MyThreadPool pool)
    {
        private readonly Queue<T> buffer = new();
        private readonly MyThreadPool myThreadPool = pool;

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
                    if (this.myThreadPool.isShutdown)
                    {
                        throw new OperationCanceledException();
                    }
                }

                return this.buffer.Dequeue();
            }
        }

        public void WakeAll()
        {
            lock (this.buffer)
            {
                Monitor.PulseAll(this.buffer);
            }
        }
    }

    private class MyTask<TResult> :
        IMyTask<TResult>
    {
        private readonly MyThreadPool threadPool;
        private readonly TaskQueue<Action> continuations;
        private readonly ManualResetEvent completionEvent = new ManualResetEvent(false);
        private TResult? result;
        private Exception? exception;
        private Func<TResult> func;

        public MyTask(MyThreadPool threadPool, Func<TResult> func)
        {
            this.threadPool = threadPool;
            this.continuations = new TaskQueue<Action>(threadPool);
            this.func = func;
        }

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
                this.result = this.func();
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