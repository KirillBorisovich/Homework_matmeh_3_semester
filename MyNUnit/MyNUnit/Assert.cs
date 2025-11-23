// <copyright file="Assert.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

namespace MyNUnit;

/// <summary>
/// Provides a set of static methods for checking conditions in MyNUnit.
/// </summary>
public static class Assert
{
    /// <summary>
    /// Check that the two specified objects are equal.
    /// </summary>
    /// <param name="expected">The expected value.</param>
    /// <param name="actual">The actual value.</param>
    /// <exception cref="AssertFailedException">Thrown when <paramref name="expected"/> and <paramref name="actual"/> are not equal.</exception>
    public static void AreEqual(object expected, object actual)
    {
        if (!Equals(expected, actual))
        {
            throw new AssertFailedException($"Expected: {expected}\n " +
                                            $"But was: {actual}\n");
        }
    }

    /// <summary>
    /// Check whether the specified condition is met.
    /// </summary>
    /// <param name="value">The condition to verify.</param>
    /// <exception cref="AssertFailedException">Thrown when <paramref name="value"/> is false.</exception>
    public static void IsTrue(bool value)
    {
        if (!value)
        {
            throw new AssertFailedException("Value is not true\n");
        }
    }
}