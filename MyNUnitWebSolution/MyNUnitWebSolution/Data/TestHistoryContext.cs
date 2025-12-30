// <copyright file="TestHistoryContext.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MyNUnitWebSolution.Data;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// The context of a database for storing test run history.
/// </summary>
/// <param name="options">Entity Framework Core Context Configuration Settings.</param>
public class TestHistoryContext(DbContextOptions<TestHistoryContext> options) : DbContext(options)
{
    /// <summary>
    /// Gets a set of entities representing individual test runs.
    /// </summary>
    public DbSet<TestRun> TestRuns => this.Set<TestRun>();

    /// <summary>
    /// Gets a set of entities representing the results of running tests
    /// for each individual assembly within a specific <see cref="TestRun"/>.
    /// </summary>
    public DbSet<AssemblyRun> AssemblyRuns => this.Set<AssemblyRun>();

    /// <summary>
    /// Gets a set of entities representing the output strings of MyNUnit
    /// /// for a specific test run of a single assembly <see cref="AssemblyRun"/>.
    /// </summary>
    public DbSet<TestOutputLine> TestOutputLines => this.Set<TestOutputLine>();
}
