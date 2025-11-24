using Core;

namespace TestProject1;

public class SimpleTests
{
    [BeforeClass]
    public static void InitOnce()
    {
        Console.WriteLine("BeforeClass executed");
    }

    [AfterClass]
    public static void FinishOnce()
    {
        Console.WriteLine("AfterClass executed");
    }

    [Before]
    public void SetUp()
    {
        Console.WriteLine("Before test");
    }

    [After]
    public void TearDown()
    {
        Console.WriteLine("After test");
    }

    [Test]
    public void PassedTest()
    {
        MyAssert.AreEqual(5, 2 + 3);
    }

    [Test]
    public void FailedTest()
    {
        MyAssert.AreEqual(10, 3 + 3); // упадёт
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