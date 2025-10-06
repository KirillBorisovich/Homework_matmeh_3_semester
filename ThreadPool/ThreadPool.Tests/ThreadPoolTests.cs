// <copyright file="ThreadPoolTests.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

namespace ThreadPool.Tests;

public class ThreadPoolTests
{
    private MyThreadPool threadPool;

    [SetUp]
    public void Setup()
    {
        this.threadPool = new MyThreadPool();
    }

    [TearDown]
    public void TearDown()
    {
        this.threadPool.Dispose();
    }

    [Test]
    public void SubmitTest()
    {
        var taskList = new List<IMyTask<int>>();
        var resultList = new List<int>();
        for (var i = 0; i < 100; i++)
        {
            var localI = i;
            taskList.Add(this.threadPool.Submit(() => localI * localI));
            resultList.Add(localI * localI);
        }

        Assert.Multiple(() =>
        {
            for (var i = 0; i < taskList.Count; i++)
            {
                Assert.That(taskList[i].Result, Is.EqualTo(resultList[i]));
            }
        });
    }

    [Test]
    public void SubmitMultipleThreadsTest()
    {
        var threads = new Thread[100];
        var tasks = new IMyTask<int>[100];
        var resultList = new List<int>();
        for (var i = 0; i < threads.Length; i++)
        {
            var localI = i;
            threads[i] = new Thread(() =>
            {
                tasks[localI] = this.threadPool.Submit(() => localI * localI);
            });
            resultList.Add(localI * localI);
        }

        foreach (var thread in threads)
        {
            thread.Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        Assert.Multiple(() =>
        {
            for (var i = 0; i < tasks.Length; i++)
            {
                Assert.That(tasks[i].IsCompleted, Is.True);
                Assert.That(tasks[i].Result, Is.EqualTo(resultList[i]));
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
        var task = this.threadPool.Submit<Exception>(() => throw new InvalidOperationException());
        var ex = Assert.Throws<AggregateException>(() => _ = task.Result);

        Assert.That(ex.InnerException, Is.InstanceOf<InvalidOperationException>());
    }

    [Test]
    public void ContinueWithTest()
    {
        var task = this.threadPool.Submit(() => 2 * 2);
        var continueTask1 = task.ContinueWith(x => x * 2);
        var continueTask2 = continueTask1.ContinueWith(x => x * 2);
        Assert.That(continueTask2.Result, Is.EqualTo(16));
    }

    [Test]
    public void ContinueWithExceptionTest()
    {
        var task = this.threadPool.Submit(() =>
        {
            Thread.Sleep(1000);
            throw new InvalidOperationException();
#pragma warning disable CS0162
            return 2;
#pragma warning restore CS0162
        });
        var continueTask = task.ContinueWith(x => x * x);

        Assert.Multiple(() =>
        {
            var ex = Assert.Throws<AggregateException>(() => _ = continueTask.Result);

            Assert.That(ex?.InnerException, Is.InstanceOf<InvalidOperationException>());
        });
    }

    [Test]
    public void ShutdownTest()
    {
        var taskCounter = 0;
        const int maxTaskNumber = 1000;
        for (var i = 0; i < maxTaskNumber; i++)
        {
            this.threadPool.Submit(() =>
            {
                Thread.Sleep(100);
                Interlocked.Increment(ref taskCounter);
                return true;
            });
        }

        Thread.Sleep(100);
        this.threadPool.Shutdown();
        Assert.That(taskCounter, Is.LessThan(maxTaskNumber));
    }

    [Test]
    public void ContinueWithAtShutdownTest()
    {
        var task = this.threadPool.Submit(() =>
        {
            Thread.Sleep(100);
            return 4;
        });
        Thread.Sleep(100);
        this.threadPool.Shutdown();
        var continueTask = task.ContinueWith(x => x * x);

        var ex = Assert.Throws<AggregateException>(() => _ = continueTask.Result);

        Assert.That(ex?.InnerException, Is.InstanceOf<OperationCanceledException>());
    }
}