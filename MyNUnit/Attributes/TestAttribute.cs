// <copyright file="TestAttribute.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

namespace Attributes;

/// <summary>
/// An attribute indicating the method under test.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class TestAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestAttribute"/> class.
    /// </summary>
    public TestAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TestAttribute"/> class.
    /// </summary>
    /// <param name="exceptionType">Expected type of exception.</param>
    public TestAttribute(Type exceptionType)
    {
        this.Expected = exceptionType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TestAttribute"/> class.
    /// </summary>
    /// <param name="ignoreReason">The reason for canceling the test run.</param>
    public TestAttribute(string ignoreReason)
    {
        this.Ignore = ignoreReason;
    }

    /// <summary>
    /// Gets expected type of exception.
    /// </summary>
    public Type? Expected { get; }

    /// <summary>
    /// Gets the reason for canceling the test run.
    /// </summary>
    public string? Ignore { get; }
}