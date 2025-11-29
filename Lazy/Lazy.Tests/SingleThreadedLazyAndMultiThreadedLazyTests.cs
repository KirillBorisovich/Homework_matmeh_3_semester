namespace Lazy.Tests;

[TestFixture]
public class SingleThreadedLazyAndMultiThreadedLazyTests
{
    public static IEnumerable<TestCaseData> IntLazyFactories
    {
        get
        {
            yield return new TestCaseData(
                new Func<Func<int>, ILazy<int>>(f => new SingleThreadedLazy<int>(f)))
                .SetName("SingleThreadedLazy<int>");
            yield return new TestCaseData(
                new Func<Func<int>, ILazy<int>>(f => new MyMultiThreadLazy<int>(f)))
                .SetName("MyMultiThreadLazy<int>");
        }
    }

    public static IEnumerable<TestCaseData> ObjectLazyFactories
    {
        get
        {
            yield return new TestCaseData(
                new Func<Func<object?>, ILazy<object?>>(f => new SingleThreadedLazy<object>(f)))
                .SetName("SingleThreadedLazy<object?>");
            yield return new TestCaseData(
                new Func<Func<object?>, ILazy<object?>>(f => new MyMultiThreadLazy<object>(f)))
                .SetName("MyMultiThreadLazy<object?>");
        }
    }

    [TestCaseSource(nameof(IntLazyFactories))]
    public void GetReturnsCorrectValue(Func<Func<int>, ILazy<int>> lazyFactory)
    {
        var lazy = lazyFactory(() => 10);

        Assert.That(lazy.Get(), Is.EqualTo(10));
    }

    [TestCaseSource(nameof(IntLazyFactories))]
    public void GetMultipleTimesReturnsSameValue(Func<Func<int>, ILazy<int>> lazyFactory)
    {
        var counter = 0;
        var lazy = lazyFactory(() =>
        {
            counter++;
            return 10;
        });

        Assert.Multiple(() =>
        {
            Assert.That(lazy.Get(),  Is.EqualTo(10));
            Assert.That(lazy.Get(),  Is.EqualTo(10));
            Assert.That(lazy.Get(), Is.EqualTo(10));
            Assert.That(counter, Is.EqualTo(1));
        });
    }

    [TestCaseSource(nameof(ObjectLazyFactories))]
    public void GetReturnsNull(Func<Func<object?>, ILazy<object?>> lazyFactory)
    {
        var counter = 0;
        var lazy = lazyFactory(() =>
        {
            counter++;
            return null;
        });
        Assert.Multiple(() =>
        {
            Assert.That(lazy.Get(), Is.Null);
            Assert.That(lazy.Get(), Is.Null);
            Assert.That(lazy.Get(), Is.Null);
            Assert.That(counter, Is.EqualTo(1));
        });
    }

    [TestCaseSource(nameof(IntLazyFactories))]
    public void ConstructorShouldThrowArgumentNullExceptionIfTheFunctionIsNull(
        Func<Func<int>, ILazy<int>> lazyFactory)
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            _ = lazyFactory(null!);
        });
    }
}

