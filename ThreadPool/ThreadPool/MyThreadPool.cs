// <copyright file="MyThreadPool.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

namespace ThreadPool;

public class MyThreadPool
{
    private readonly TaskQueue<Action> taskQueue = new();
    private readonly CancellationTokenSource cancellationTokenSource = new();
    private readonly Thread[] threads;

    public MyThreadPool()
    {
        this.ThreadsCount = Environment.ProcessorCount;
        this.threads = new Thread[this.ThreadsCount];
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

    private int ThreadsCount { get; }

    public IMyTask<TResult> Submit<TResult>(Func<TResult> func)
    {
        var task = new MyTask<TResult>(this, func);
        this.Run(() => task.Execute());
        return task;
    }

    public void Shutdown()
    {
        this.cancellationTokenSource.Cancel();
        foreach (var thread in this.threads)
        {
            thread.Join();
        }
    }

    private void Run(Action action)
    {
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

    private class MyTask<TResult>(MyThreadPool threadPool, Func<TResult> func) : IMyTask<TResult>
    {
        private readonly MyThreadPool threadPool = threadPool;
        private readonly TaskQueue<Action> taskQueue = new();
        private readonly ManualResetEvent completionEvent = new ManualResetEvent(false);
        private TResult? result = default;

        public bool IsCompleted { get; } = false;

        public TResult Result
        {
            get
            {
                this.completionEvent.WaitOne();

                return this.result!;
            }
        }

        public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> function)
        {
            var newTask = new MyTask<TNewResult>(this.threadPool, () => function(this.Result));
            this.taskQueue.Enqueue(newTask.Execute);
            return newTask;
        }

        public void Execute()
        {
            this.result = func();
            this.completionEvent.Set();

            if (this.taskQueue.Count == 0)
            {
                return;
            }

            for (var i = 0; i < this.taskQueue.Count; i++)
            {
                this.threadPool.Run(this.taskQueue.Dequeue());
            }
        }
    }
}