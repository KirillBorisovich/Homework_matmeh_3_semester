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
        var threadPool = new MyThreadPool();
        var taskList = new List<IMyTask<int>>();
        var resultList = new List<int>();
        for (var i = 0; i < 100; i++)
        {
            var localI = i;
            taskList.Add(threadPool.Submit(() => localI * 2));
            resultList.Add(localI * 2);
        }

        Assert.Multiple(() =>
        {
            for (var i = 0; i < taskList.Count; i++)
            {
                Assert.That(taskList[i].Result, Is.EqualTo(resultList[i]));
            }
        });
    }
}
