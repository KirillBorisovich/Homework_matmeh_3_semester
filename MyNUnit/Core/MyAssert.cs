// <copyright file="MyAssert.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

namespace Core;

/// <summary>
/// Provides a set of static methods for checking conditions in MyNUnit.
/// </summary>
public static class MyAssert
{
    /// <summary>
    /// Check that the two specified objects are equal.
    /// </summary>
    /// <param name="expected">The expected value.</param>
    /// <param name="actual">The actual value.</param>
    /// <exception>Thrown when
    ///     <cref>AssertFailedException</cref>
    ///     <paramref name="expected"/> and <paramref name="actual"/> are not equal.</exception>
    public static void AreEqual(object expected, object actual)
    {
        if (!Equals(expected, actual))
        {
            throw new AssertFailedException($"    Expected: {expected}\n " +
                                            $"   But was: {actual}");
        }
    }

    /// <summary>
    /// Check whether the specified condition is met.
    /// </summary>
    /// <param name="value">The condition to verify.</param>
    /// <exception>Thrown when
    ///     <cref>AssertFailedException</cref>
    ///     <paramref name="value"/> is false.</exception>
    public static void IsTrue(bool value)
    {
        if (!value)
        {
            throw new AssertFailedException("    Value is not true");
        }
    }
}