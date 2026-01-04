// <copyright file="ILazy.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

namespace Lazy;

/// <summary>
/// Interface for lazy calculation.
/// </summary>
/// <typeparam name="T">Parameter type of the function result.</typeparam>
public interface ILazy<out T>
{
    T? Get();
}