// <copyright file="TestOutputLine.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MyNUnitWebSolution.Data;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Represents a single line (or block) of textual output produced by MyNUnit
/// for a particular assembly run.
/// </summary>
public class TestOutputLine
{
    /// <summary>
    /// Gets the primary key of the output line record.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Gets the foreign key of the related <see cref="AssemblyRun"/>.
    /// </summary>
    public int AssemblyRunId { get; init; }

    /// <summary>
    /// Gets the <see cref="Data.AssemblyRun"/> to which this output line belongs.
    /// </summary>
    public AssemblyRun AssemblyRun { get; init; } = null!;

    /// <summary>
    /// Gets the position of this line within the sequence of lines
    /// for the corresponding <see cref="AssemblyRun"/>.
    /// </summary>
    public int Order { get; init; }

    /// <summary>
    /// Gets the raw text of the output line.
    /// </summary>
    [MaxLength(4000)]
    public string Text { get; init; } = null!;
}