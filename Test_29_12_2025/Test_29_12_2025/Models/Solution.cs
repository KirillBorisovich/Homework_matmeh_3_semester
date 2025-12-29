// <copyright file="Solution.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

namespace Test_29_12_2025.Models;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// The model for solution.
/// </summary>
public class Solution
{
    /// <summary>
    /// Gets or sets key.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets task id.
    /// </summary>
    public int TaskId { get; set; }

    /// <summary>
    /// Gets or sets homework task.
    /// </summary>
    public HomeworkTask? Task { get; set; }

    /// <summary>
    /// Gets or sets submission date.
    /// </summary>
    public DateTime SubmissionDate { get; set; }

    /// <summary>
    /// Gets or sets text.
    /// </summary>
    [MaxLength(4000)]
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets score.
    /// </summary>
    public int Score { get; set; }
}