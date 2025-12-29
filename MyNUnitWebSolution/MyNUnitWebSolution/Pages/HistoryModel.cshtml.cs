namespace MyNUnitWebSolution.Pages;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyNUnitWebSolution.Data;

public class HistoryModel : PageModel
{
    private readonly TestHistoryContext db;

    public HistoryModel(TestHistoryContext db)
    {
        this.db = db;
    }

    public List<AssemblySummary> Assemblies { get; private set; } = new();

    public async Task OnGetAsync()
    {
        Assemblies = await db.AssemblyRuns
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

    public class AssemblySummary
    {
        public string? AssemblyName { get; init; }

        public int RunsCount { get; init; }

        public int TotalPassed { get; init; }

        public int TotalFailed { get; init; }

        public int TotalIgnored { get; init; }
    }

    public async Task<IActionResult> OnGetAssemblyHistoryAsync(string assemblyName)
    {
        if (string.IsNullOrWhiteSpace(assemblyName))
        {
            return this.BadRequest();
        }

        var runs = await db.AssemblyRuns
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
}