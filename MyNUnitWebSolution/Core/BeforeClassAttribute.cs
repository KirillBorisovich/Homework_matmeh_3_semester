// <copyright file="BeforeClassAttribute.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

namespace Core;

/// <summary>
/// An attribute indicating the method to be called before executing all tests in the class.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class BeforeClassAttribute : Attribute;