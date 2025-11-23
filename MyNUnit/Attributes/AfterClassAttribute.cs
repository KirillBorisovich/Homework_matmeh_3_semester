// <copyright file="AfterClassAttribute.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

namespace Attributes;

/// <summary>
/// An attribute indicating the method that will be called after all tests in the class have been performed.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class AfterClassAttribute : Attribute;