// <copyright file="HomeworkTask.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

namespace Test_29_12_2025.Models;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// The model for homework task.
/// </summary>
public class HomeworkTask
{
    /// <summary>
    /// Gets or sets key.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets homeworkId.
    /// </summary>
    public int HomeworkId { get; set; }

    /// <summary>
    /// Gets or sets homework.
    /// </summary>
    public Homework? Homework { get; set; }

    /// <summary>
    /// Gets or sets description.
    /// </summary>
    [MaxLength(4000)]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets max score.
    /// </summary>
    public int MaxScore { get; set; }

    /// <summary>
    /// Gets or sets solutions.
    /// </summary>
    public List<Solution> Solutions { get; set; } = [];
}