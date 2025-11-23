// <copyright file="AfterAttribute.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

namespace Attributes;

/// <summary>
/// An attribute indicating the method to be called after the test is completed.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class AfterAttribute : Attribute;