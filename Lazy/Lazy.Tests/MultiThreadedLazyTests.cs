// <copyright file="MultiThreadedLazyTests.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

namespace Lazy.Tests;

public class MultiThreadedLazyTests
{
    [Test]
    [CancelAfter(2000)]
    public void MultiThreadedTest()
    {
        const int threadsCount = 10;
        var counter = 0;
        var lazy = new MyMultiThreadLazy<int>(() =>
        {
            Interlocked.Increment(ref counter);
            return 20;
        });
        var threads = new Thread[threadsCount];
        var results = new int[threadsCount];
        for (var i = 0; i < 10; i++)
        {
            var i1 = i;
            threads[i] = new Thread(() => results[i1] = lazy.Get());
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
            foreach (var item in results)
            {
                Assert.That(item, Is.EqualTo(20));
            }

            Assert.That(counter, Is.EqualTo(1));
        });
    }
}