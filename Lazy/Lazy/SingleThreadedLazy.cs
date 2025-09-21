// <copyright file="SingleThreadedLazy.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

namespace Lazy;

/// <summary>
/// Lazy function calculation for single-threaded use.
/// </summary>
/// <param name="supplier">A function for calculating.</param>
/// <typeparam name="T">Parameter type of the function result.</typeparam>
public class SingleThreadedLazy<T>(Func<T>? supplier) : ILazy<T>
{
    private Func<T>? supplier = supplier ?? throw new ArgumentNullException(nameof(supplier));
    private T? result;
    private bool isCalculated;

    /// <summary>
    /// Get the calculation result.
    /// </summary>
    /// <returns>Calculation result.</returns>
    public T? Get()
    {
        if (this.isCalculated)
        {
            return this.result;
        }

        var func = this.supplier;
        if (func != null)
        {
            this.result = func();
            this.isCalculated = true;
        }

        this.supplier = null;

        return this.result;
    }
}