// <copyright file="BeforeAttribute.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

namespace Attributes;

/// <summary>
/// An attribute indicating the method to be called before executing the test.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class BeforeAttribute : Attribute;
