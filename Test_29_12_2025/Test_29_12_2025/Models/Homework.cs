// <copyright file="Homework.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

namespace Test_29_12_2025.Models;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// The model for Homework.
/// </summary>
public class Homework
{
    /// <summary>
    /// Gets or sets key.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets title.
    /// </summary>
    [MaxLength(4000)]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets deadline.
    /// </summary>
    public DateTime Deadline { get; set; }

    /// <summary>
    /// Gets or sets tasks.
    /// </summary>
    public List<HomeworkTask> Tasks { get; set; } = [];
}