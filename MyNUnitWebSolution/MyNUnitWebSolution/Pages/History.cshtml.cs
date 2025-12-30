// <copyright file="History.cshtml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MyNUnitWebSolution.Pages;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyNUnitWebSolution.Data;

/// <summary>
/// Page model for the history view.
/// Provides aggregated information about all test runs stored in the
/// <see cref="TestHistoryContext"/> database and exposes endpoints
/// for retrieving detailed history per assembly.
/// </summary>
public class History : PageModel
{
    private readonly TestHistoryContext db;

    /// <summary>
    /// Initializes a new instance of the <see cref="History"/> class.
    /// </summary>
    /// <param name="db">
    /// The EF Core context used to query stored test run history.
    /// </param>
    public History(TestHistoryContext db)
    {
        this.db = db;
    }

    /// <summary>
    /// Gets the aggregated statistics per assembly:
    /// total number of runs, and total counts of passed, failed and ignored tests.
    /// Populated in <see cref="OnGetAsync"/>.
    /// </summary>
    public List<AssemblySummary> Assemblies { get; private set; } = new();

    /// <summary>
    /// Handles GET requests for the history page.
    /// Loads aggregate statistics for all assemblies that have ever been executed,
    /// grouping data across all test runs.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task OnGetAsync()
    {
        this.Assemblies = await this.db.AssemblyRuns
            .Select(a => new
            {
                a.AssemblyName,
                a.TestRunId,
                Passed = a.Lines.Count(l =>
                    l.Text.Contains("Test Passed:")),
                Failed = a.Lines.Count(l =>
                    l.Text.Contains("Test Failed:") ||
                    l.Text.Contains("An exception is raised in:")),
                Ignored = a.Lines.Count(l =>
                    l.Text.Contains("Test Ignored:")),
            })
            .GroupBy(x => x.AssemblyName)
            .Select(g => new AssemblySummary
            {
                AssemblyName = g.Key,
                RunsCount = g.Select(x => x.TestRunId).Distinct().Count(),
                TotalPassed = g.Sum(x => x.Passed),
                TotalFailed = g.Sum(x => x.Failed),
                TotalIgnored = g.Sum(x => x.Ignored),
            })
            .OrderBy(a => a.AssemblyName)
            .ToListAsync();
    }

    /// <summary>
    /// AJAX endpoint that returns the detailed history for a specific assembly:
    /// a list of runs with their start time and the raw output lines for each run.
    /// </summary>
    /// <param name="assemblyName">
    /// The name of the assembly (DLL) for which detailed history should be returned.
    /// </param>
    /// <returns>
    /// A <see cref="JsonResult"/> containing a list of runs for the given assembly.
    /// Each run object includes the run start time and the ordered collection of
    /// textual output lines produced by MyNUnit.
    /// </returns>
    public async Task<IActionResult> OnGetAssemblyHistoryAsync(string assemblyName)
    {
        if (string.IsNullOrWhiteSpace(assemblyName))
        {
            return this.BadRequest();
        }

        var runs = await this.db.AssemblyRuns
            .Where(a => a.AssemblyName == assemblyName)
            .Select(a => new
            {
                a.Id,
                RunStartedAt = a.TestRun.StartedAt,
                Lines = a.Lines
                    .OrderBy(l => l.Order)
                    .Select(l => l.Text)
                    .ToList(),
            })
            .OrderBy(r => r.RunStartedAt)
            .ToListAsync();

        return new JsonResult(runs);
    }

    /// <summary>
    /// Represents aggregated statistics for a single assembly across
    /// all stored test runs.
    /// </summary>
    public class AssemblySummary
    {
        /// <summary>
        /// Gets the name of the assembly (DLL) for which statistics are calculated.
        /// </summary>
        public string? AssemblyName { get; init; }

        /// <summary>
        /// Gets the number of distinct test runs in which this assembly participated.
        /// </summary>
        public int RunsCount { get; init; }

        /// <summary>
        /// Gets the total number of passed tests for this assembly across all runs.
        /// </summary>
        public int TotalPassed { get; init; }

        /// <summary>
        /// Gets the total number of failed tests for this assembly across all runs.
        /// This includes both explicit failures and test methods where an exception was raised.
        /// </summary>
        public int TotalFailed { get; init; }

        /// <summary>
        /// Gets the total number of ignored tests for this assembly across all runs.
        /// </summary>
        public int TotalIgnored { get; init; }
    }
}