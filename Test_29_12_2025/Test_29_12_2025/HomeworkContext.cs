// <copyright file="HomeworkContext.cs" company="Bengya Kirill">
// Copyright (c) Bengya Kirill under MIT License.
// </copyright>

namespace Test_29_12_2025;

using Microsoft.EntityFrameworkCore;
using Test_29_12_2025.Models;

/// <summary>
/// Homework context.
/// </summary>
public class HomeworkContext : DbContext
{
    /// <summary>
    /// Gets or sets homeworks.
    /// </summary>
    public DbSet<Homework> Homeworks { get; set; } = null!;

    /// <summary>
    /// Gets or sets tasks.
    /// </summary>
    public DbSet<HomeworkTask> Tasks { get; set; } = null!;

    /// <summary>
    /// Gets or sets solutions.
    /// </summary>
    public DbSet<Solution> Solutions { get; set; } = null!;

    /// <inheritdoc/>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=homeworks.db");
    }

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Homework>()
            .HasMany(h => h.Tasks)
            .WithOne(t => t.Homework)
            .HasForeignKey(t => t.HomeworkId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<HomeworkTask>()
            .HasMany(t => t.Solutions)
            .WithOne(s => s.Task)
            .HasForeignKey(s => s.TaskId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}