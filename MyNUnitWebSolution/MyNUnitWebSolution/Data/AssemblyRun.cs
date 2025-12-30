// <copyright file="AssemblyRun.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MyNUnitWebSolution.Data;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// A model for storing the result of a single test run of a specific assembly.
/// </summary>
public class AssemblyRun
{
    /// <summary>
    /// Gets key.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Gets TestRunId.
    /// </summary>
    public int TestRunId { get; init; }

    /// <summary>
    /// Gets TestRun.
    /// </summary>
    public TestRun TestRun { get; init; } = null!;

    /// <summary>
    /// Gets AssemblyName.
    /// </summary>
    [MaxLength(4000)]
    public string AssemblyName { get; init; } = null!;

    /// <summary>
    /// Gets Lines.
    /// </summary>
    public ICollection<TestOutputLine> Lines { get; init; } = new List<TestOutputLine>();
}