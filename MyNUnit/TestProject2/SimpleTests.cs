using Core;

namespace TestProject2;

public class SimpleTests
{
    [AfterClass]
    public static void FinishOnce()
    {
        throw new InvalidOperationException();
    }

    [Before]
    public void SetUp()
    {
    }

    [After]
    public void TearDown()
    {
    }

    [Test]
    public void PassedTest()
    {
        MyAssert.AreEqual(5, 2 + 3);
    }

    [Test]
    public void FailedTest()
    {
        MyAssert.AreEqual(10, 3 + 3);
    }

    [Test(Ignore = "Demonstration of ignored test")]
    public void IgnoredTest()
    {
    }

    [Test(Expected = typeof(InvalidOperationException))]
    public void ExpectedExceptionTest()
    {
        throw new InvalidOperationException();
    }
}