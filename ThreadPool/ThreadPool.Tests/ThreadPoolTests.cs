// <copyright file="ThreadPoolTests.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

namespace ThreadPool.Tests;

using System.Collections.Concurrent;

public class ThreadPoolTests
{
    private MyThreadPool threadPool;

    [SetUp]
    public void Setup()
        => this.threadPool = new MyThreadPool(Environment.ProcessorCount);

    [Test]
    public void SubmitTest()
    {
        var taskList = new List<IMyTask<int>>();
        for (var i = 0; i < 100; i++)
        {
            var localI = i;
            taskList.Add(this.threadPool.Submit(() => localI * localI));
        }

        Assert.Multiple(() =>
        {
            for (var i = 0; i < taskList.Count; i++)
            {
                Assert.That(taskList[i].Result, Is.EqualTo(i * i));
            }
        });
    }

    [Test]
    public void SubmitMultipleThreadsTest()
    {
        const int count = 100;
        var threads = new Thread[count];
        var tasks = new IMyTask<int>[count];

        for (var i = 0; i < count; i++)
        {
            var localI = i;
            threads[i] = new Thread(() =>
            {
                tasks[localI] = this.threadPool.Submit(() => localI * localI);
            });
            threads[i].Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        Assert.Multiple(() =>
        {
            for (var i = 0; i < count; i++)
            {
                Assert.That(tasks[i], Is.Not.Null, $"Task at index {i} was null");
                Assert.That(tasks[i].Result, Is.EqualTo(i * i));
                Assert.That(tasks[i].IsCompleted, Is.True);
            }
        });
    }

    [Test]
    public void SubmitExceptionTest()
    {
        this.threadPool.Shutdown();
        Assert.Throws<OperationCanceledException>(() =>
            this.threadPool.Submit(() => 2 * 2));
    }

    [Test]
    public void Result_WhenTaskThrowsException_ThrowsAggregateException()
    {
        var task = this.threadPool.Submit<int>(() => throw new InvalidOperationException("Boom"));
        var ex = Assert.Throws<AggregateException>(() => _ = task.Result);

        Assert.That(ex.InnerException, Is.InstanceOf<InvalidOperationException>());
        Assert.That(ex.InnerException.Message, Is.EqualTo("Boom"));
    }

    [Test]
    public void ContinueWithTest()
    {
        var task = this.threadPool.Submit(() => 2 * 2);
        var continueTask1 = task.ContinueWith(x => x * 2);
        var continueTask2 = continueTask1.ContinueWith(x => x + 2);
        Assert.That(continueTask2.Result, Is.EqualTo(10));
    }

    [Test]
    public void ContinueWithExceptionTest()
    {
        var task = this.threadPool.Submit<int>(() =>
        {
            Thread.Sleep(50);
            throw new InvalidOperationException("Original Error");
        });

        var continueTask = task.ContinueWith(x => x * x);

        var ex = Assert.Throws<AggregateException>(() => _ = continueTask.Result);

        Assert.That(ex?.InnerException, Is.InstanceOf<InvalidOperationException>());
        Assert.That(ex?.InnerException?.Message, Is.EqualTo("Original Error"));
    }

    [Test]
    public void ContinueWithExceptionInContinuationThrowsAggregateException()
    {
        var task = this.threadPool.Submit(() => 5);
        var continueTask = task.ContinueWith<int>(x => throw new ArithmeticException("Continuation Error"));

        var ex = Assert.Throws<AggregateException>(() => _ = continueTask.Result);
        Assert.That(ex?.InnerException, Is.InstanceOf<ArithmeticException>());
    }

    [Test]
    public void ShutdownTest()
    {
        var taskCounter = 0;
        const int tasksToSubmit = 100;

        using var startEvent = new ManualResetEvent(false);

        for (var i = 0; i < tasksToSubmit; i++)
        {
            try
            {
                this.threadPool.Submit(() =>
                {
                    startEvent!.WaitOne();
                    Interlocked.Increment(ref taskCounter);
                    return true;
                });
            }
            catch (OperationCanceledException)
            {
            }
        }

        this.threadPool.Shutdown();

        startEvent.Set();

        Thread.Sleep(200);

        Assert.That(taskCounter, Is.LessThan(tasksToSubmit));

        Console.WriteLine($"Executed tasks: {taskCounter} out of {tasksToSubmit}");
    }

    [Test]
    public void ContinueWithAtShutdownTest()
    {
        var task = this.threadPool.Submit(() => 42);

        Assert.That(task.Result, Is.EqualTo(42));

        this.threadPool.Shutdown();

        Assert.Throws<OperationCanceledException>(() =>
            task.ContinueWith(x => x * 2));
    }

    [Test]
    public void ContinueWithParallelShutdownRaceTest()
    {
        var task = this.threadPool.Submit(() =>
        {
            Thread.Sleep(100);
            return 10;
        });

        Task.Run(() =>
        {
            Thread.Sleep(50);
            this.threadPool.Shutdown();
        });

        try
        {
            var continuation = task.ContinueWith(x => x + 1);
            try
            {
                var res = continuation.Result;
                Assert.That(res, Is.EqualTo(11));
            }
            catch (AggregateException ae)
            {
                Assert.That(ae.InnerException, Is.InstanceOf<OperationCanceledException>());
            }
        }
        catch (OperationCanceledException)
        {
            Assert.Pass();
        }
    }

    [Test]
    public void ThreadsCountTest()
        => Assert.That(this.threadPool.ThreadsCount, Is.EqualTo(Environment.ProcessorCount));

    [Test]
    public void ThreadCountShouldUseExactlyRequestedNumberOfThreads()
    {
        const int expectedThreadCount = 4;
        var localPool = new MyThreadPool(expectedThreadCount);

        var threadIds = new ConcurrentDictionary<int, byte>();
        var countdown = new CountdownEvent(expectedThreadCount * 5);

        for (var i = 0; i < expectedThreadCount * 5; i++)
        {
            localPool.Submit(() =>
            {
                threadIds.TryAdd(Environment.CurrentManagedThreadId, 0);

                Thread.Sleep(50);
                countdown.Signal();
                return 0;
            });
        }

        countdown.Wait(TimeSpan.FromSeconds(2));

        Assert.That(threadIds.Keys, Has.Count.EqualTo(expectedThreadCount), $"Expected {expectedThreadCount} unique threads, but found {threadIds.Keys.Count}");
    }

    [Test]
    public void SubmitRaceConditionWithShutdown()
    {
        var localPool = new MyThreadPool(4);
        var keepRunning = true;
        var exceptionsCaught = 0;
        var tasksSubmitted = 0;

        var submitterThread = new Thread(() =>
        {
            while (keepRunning)
            {
                try
                {
                    localPool.Submit(() => 1);
                    tasksSubmitted++;
                }
                catch (OperationCanceledException)
                {
                    Interlocked.Increment(ref exceptionsCaught);
                    break;
                }
                catch (Exception)
                {
                    Assert.Fail("Unexpected exception type thrown during race condition");
                }
            }
        });

        submitterThread.Start();

        Thread.Sleep(50);

        keepRunning = false;
        localPool.Shutdown();

        submitterThread.Join();

        Console.WriteLine($"Submitted: {tasksSubmitted}, Caught Expected Exception: {exceptionsCaught > 0}");
        Assert.Pass("Race condition handled gracefully");
    }

    [Test]
    public void ContinueWithRaceConditionWithShutdown()
    {
        var localPool = new MyThreadPool(4);

        var rootTask = localPool.Submit(() =>
        {
            Thread.Sleep(100);
            return 42;
        });

        var raceThread = new Thread(() =>
        {
            try
            {
                Thread.Sleep(50);
                rootTask.ContinueWith(x => x + 1);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Assert.Fail($"Unexpected exception: {ex}");
            }
        });

        raceThread.Start();

        Thread.Sleep(50);
        localPool.Shutdown();

        raceThread.Join();
        Assert.Pass("ContinueWith race handled gracefully");
    }
}