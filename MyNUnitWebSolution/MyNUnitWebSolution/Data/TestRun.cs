// <copyright file="TestRun.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MyNUnitWebSolution.Data;

/// <summary>
/// Represents a single execution (run) of the test suite,
/// which may include multiple assemblies being tested.
/// </summary>
public class TestRun
{
    /// <summary>
    /// Gets the primary key of the test run record.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Gets the UTC date and time when the test run started.
    /// </summary>
    public DateTime StartedAt { get; init; }

    /// <summary>
    /// Gets the UTC date and time when the test run finished.
    /// </summary>
    public DateTime FinishedAt { get; init; }

    /// <summary>
    /// Gets the collection of assembly-level results that belong to this test run.
    /// Each <see cref="AssemblyRun"/> represents the results for a single assembly (DLL).
    /// </summary>
    public ICollection<AssemblyRun> Assemblies { get; init; } = new List<AssemblyRun>();
}