// <copyright file="AssertFailedException.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

namespace Core;

/// <summary>
/// Exceptions about a failed test.
/// </summary>
/// <param name="message">Exceptions messages.</param>
public class AssertFailedException(string message) : Exception(message);