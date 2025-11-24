// <copyright file="MyNUnitClassTest.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

namespace MyNUnit.Tests;

public class MyNUnitClassTest
{
    private MyNUnitClass myNUnitClass;

    [SetUp]
    public void Setup()
    {
        this.myNUnitClass = new MyNUnitClass();
    }

    [Test]
    public void RunAllTheTestsAlongThisPathTest()
    {
    }
}