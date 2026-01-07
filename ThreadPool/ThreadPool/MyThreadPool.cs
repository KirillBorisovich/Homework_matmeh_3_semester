// <copyright file="MyThreadPool.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

namespace ThreadPool;

/// <summary>
/// Author's ThreadPool.
/// </summary>
public class MyThreadPool
{
    private readonly Lock lockObject = new();
    private readonly TaskQueue<Action> taskQueue;
    private readonly Thread[] threads;
    private bool isShutdown;

    /// <summary>
    /// Initializes a new instance of the <see cref="MyThreadPool"/> class.
    /// </summary>
    /// <param name="threadCount">The number of threads used in the MyThreadPool.</param>
    public MyThreadPool(int threadCount)
    {
        this.taskQueue = new TaskQueue<Action>(this, true);
        this.threads = new Thread[threadCount];
        for (var i = 0; i < this.threads.Length; i++)
        {
            this.threads[i] = new Thread(this.Task)
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
        lock (this.lockObject)
        {
            if (this.isShutdown)
            {
                return;
            }

            this.isShutdown = true;
            this.taskQueue.WakeAll();
            foreach (var thread in this.threads)
            {
                thread.Join();
            }
        }
    }

    private void Run(Action action)
    {
        if (this.isShutdown)
        {
            throw new OperationCanceledException("ThreadPool is stopped");
        }

        this.taskQueue.Enqueue(action);
    }

    private void Task()
    {
        while (true)
        {
            if (!this.taskQueue.TryDequeue(out var task))
            {
                return;
            }

            try
            {
                task?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Uncaught exception in thread pool task: {ex}");
            }
        }
    }

    private class TaskQueue<T>(MyThreadPool pool, bool thisIsAConsumerAndProducerProblem)
    {
        private readonly Queue<T> buffer = new();

        public void Enqueue(T item)
        {
            lock (this.buffer)
            {
                if (pool.isShutdown)
                {
                    throw new OperationCanceledException("ThreadPool is stopped");
                }

                this.buffer.Enqueue(item);
                if (thisIsAConsumerAndProducerProblem)
                {
                    Monitor.Pulse(this.buffer);
                }
            }
        }

        public bool TryDequeue(out T? result)
        {
            lock (this.buffer)
            {
                while (this.buffer.Count == 0 && thisIsAConsumerAndProducerProblem)
                {
                    if (pool.isShutdown)
                    {
                        result = default;
                        return false;
                    }

                    Monitor.Wait(this.buffer);
                }

                if (this.buffer.Count == 0)
                {
                    result = default;
                    return false;
                }

                result = this.buffer.Dequeue();
                return true;
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

    private class MyTask<TResult>(MyThreadPool threadPool, Func<TResult> func) :
        IMyTask<TResult>
    {
        private readonly Lock lockObject = new();
        private readonly TaskQueue<Action> continuations = new(threadPool, false);
        private readonly ManualResetEvent completionEvent = new(false);
        private TResult? result;
        private Exception? exception;

        public bool IsCompleted { get; private set; }

        public TResult Result
        {
            get
            {
                this.completionEvent.WaitOne();
                return this.exception != null ? throw new AggregateException(this.exception) : this.result!;
            }
        }

        public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> function)
        {
            if (threadPool.isShutdown)
            {
                throw new OperationCanceledException("ThreadPool is stopped");
            }

            var newTask = new MyTask<TNewResult>(threadPool, () => function(this.Result));

            lock (this.lockObject)
            {
                if (threadPool.isShutdown)
                {
                    throw new OperationCanceledException("ThreadPool is stopped");
                }

                if (!this.IsCompleted)
                {
                    this.continuations.Enqueue(ExecuteForContinue);
                    return newTask;
                }
            }

            threadPool.Run(ExecuteForContinue);

            return newTask;

            void ExecuteForContinue()
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
            }
        }

        public void Execute()
        {
            if (threadPool.isShutdown)
            {
                HandleCancellation();
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

            lock (this.lockObject)
            {
                this.IsCompleted = true;
                this.completionEvent.Set();
            }

            if (!threadPool.isShutdown)
            {
                try
                {
                    this.SendAllContinuationsToTheThreadPool();
                    return;
                }
                catch (OperationCanceledException)
                {
                }
            }

            this.ExecuteAllContinuations();
            return;

            void HandleCancellation()
            {
                this.exception = new OperationCanceledException("ThreadPool is stopped");
                lock (this.lockObject)
                {
                    this.IsCompleted = true;
                    this.completionEvent.Set();
                }

                this.ExecuteAllContinuations();
            }
        }

        private void SendAllContinuationsToTheThreadPool()
        {
            while (this.continuations.TryDequeue(out var continuation))
            {
                threadPool.Run(continuation!);
            }
        }

        private void ExecuteAllContinuations()
        {
            while (this.continuations.TryDequeue(out var continuation))
            {
                continuation!.Invoke();
            }
        }
    }
}