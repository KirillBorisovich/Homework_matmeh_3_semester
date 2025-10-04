// <copyright file="SingleThreadedLazyTests.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

namespace Lazy.Tests;

public class SingleThreadedLazyTests
{
    [Test]
    public void GetReturnsCorrectValue()
    {
        var singleThreaded = new SingleThreadedLazy<int>(() => 10);
        var multiThreaded = new MyMultiThreadLazy<int>(() => 10);

        Assert.Multiple(() =>
        {
            Assert.That(singleThreaded.Get(), Is.EqualTo(10));
            Assert.That(multiThreaded.Get(), Is.EqualTo(10));
        });
    }

    [Test]
    public void GetMultipleTimesReturnsSameValue()
    {
        var counter1 = 0;
        var counter2 = 0;
        var singleThreaded = new SingleThreadedLazy<int>(() =>
        {
            counter1++;
            return 10;
        });
        var multiThreaded = new MyMultiThreadLazy<int>(() =>
        {
            counter2++;
            return 20;
        });
        Assert.Multiple(() =>
        {
            Assert.That(singleThreaded.Get(),  Is.EqualTo(10));
            Assert.That(singleThreaded.Get(),  Is.EqualTo(10));
            Assert.That(singleThreaded.Get(), Is.EqualTo(10));
            Assert.That(counter1, Is.EqualTo(1));
            Assert.That(multiThreaded.Get(),  Is.EqualTo(20));
            Assert.That(multiThreaded.Get(),  Is.EqualTo(20));
            Assert.That(multiThreaded.Get(), Is.EqualTo(20));
            Assert.That(counter2, Is.EqualTo(1));
        });
    }

    [Test]
    public void GetReturnsNull()
    {
        var counter1 = 0;
        var counter2 = 0;
        var singleThreaded = new SingleThreadedLazy<object?>(() =>
        {
            counter1++;
            return null;
        });
        var multiThreaded = new MyMultiThreadLazy<object?>(() =>
        {
            counter2++;
            return null;
        });
        Assert.Multiple(() =>
        {
            Assert.That(singleThreaded.Get(), Is.Null);
            Assert.That(singleThreaded.Get(), Is.Null);
            Assert.That(singleThreaded.Get(), Is.Null);
            Assert.That(counter1, Is.EqualTo(1));
            Assert.That(multiThreaded.Get(), Is.Null);
            Assert.That(multiThreaded.Get(), Is.Null);
            Assert.That(multiThreaded.Get(), Is.Null);
            Assert.That(counter2, Is.EqualTo(1));
        });
    }

    [Test]
    public void ConstructorShouldThrowArgumentNullExceptionIfTheFunctionIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            var unused = new SingleThreadedLazy<int>(null);
        });
        Assert.Throws<ArgumentNullException>(() =>
        {
            var unused = new MyMultiThreadLazy<int>(null);
        });
    }
}