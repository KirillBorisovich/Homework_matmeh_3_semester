// <copyright file="MyNUnitClassTest.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

namespace MyNUnit.Tests;

using System.Collections.Concurrent;

public class MyNUnitClassTest
{
    private readonly ConcurrentBag<string> simpleBag =
    [
        "\nTest Passed: ExpectedExceptionTest\n",
        "\nTest Failed: FailedTest\n    Expected: 10\n    But was: 6\n",
        "\nTest Passed: PassedTest\n",
        "\nTest Ignored: IgnoredTest\n    Reason: Demonstration of ignored test\n",
        "\nAn exception is raised in: FinishOnce\n    Exception: InvalidOperationException\n    " +
        "Message: Operation is not valid due to the current state of the object.\n"
    ];

    private MyNUnitClass myNUnitClass;

    [SetUp]
    public void Setup()
    {
        this.myNUnitClass = new MyNUnitClass();
    }

    [Test]
    public async Task RunAllTheTestsAlongThisPathTest()
    {
        var result = (await this.myNUnitClass.RunAllTheTestsAlongThisPath(
            "../../../../TestsAssemblies/")).Select(CleanTime);

        var areEqual = this.simpleBag.OrderBy(x => x)
            .SequenceEqual(result.OrderBy(x => x));
        Assert.That(areEqual,  Is.True);
    }

    [Test]
    public void RunAllTheTestsAlongThisPathTheUncorrectedPathThrowsArgumentException()
    {
        Assert.Multiple(() =>
        {
            Assert.ThrowsAsync<ArgumentException>(async () => await this.myNUnitClass.RunAllTheTestsAlongThisPath(
                "./Testasdfr/"));
            Assert.ThrowsAsync<ArgumentException>(async () => await this.myNUnitClass.RunAllTheTestsAlongThisPath(
                "./Core.pdb"));
        });
    }

    private static string CleanTime(string input) =>
        string.Join(
            "\n",
            input
                .Split('\n')
                .Where(line => !line.Contains("Time")));
}