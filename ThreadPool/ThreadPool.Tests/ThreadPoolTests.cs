namespace ThreadPool.Tests;

public class ThreadPoolTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void SubmitTest()
    {
        using var threadPool = new MyThreadPool();
        var taskList = new List<IMyTask<int>>();
        var resultList = new List<int>();
        for (var i = 0; i < 100; i++)
        {
            var localI = i;
            taskList.Add(threadPool.Submit(() => localI * localI));
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
        using var threadPool = new MyThreadPool();
        var threads = new Thread[100];
        var tasks = new IMyTask<int>[100];
        var resultList = new List<int>();
        for (var i = 0; i < threads.Length; i++)
        {
            var localI = i;
            threads[i] = new Thread(() =>
            {
                tasks[localI] = threadPool.Submit(() => localI * localI);
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
                Assert.That(tasks[i].Result, Is.EqualTo(resultList[i]));
            }
        });
    }

    [Test]
    public void ShutdownTest()
    {
        using var threadPool = new MyThreadPool();
        var taskCounter = 0;
        const int maxTaskNumber = 1000;
        for (var i = 0; i < maxTaskNumber; i++)
        {
            threadPool.Submit(() =>
            {
                Thread.Sleep(100);
                Interlocked.Increment(ref taskCounter);
                return true;
            });
        }

        Thread.Sleep(100);
        Assert.That(taskCounter, Is.LessThan(maxTaskNumber));
    }
}
