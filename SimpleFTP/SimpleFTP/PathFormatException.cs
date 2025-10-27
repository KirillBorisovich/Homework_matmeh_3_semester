// <copyright file="PathFormatException.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

namespace SimpleFTP;

/// <summary>
/// Incorrect path format.
/// </summary>
public class PathFormatException(string message) : FormatException(message);