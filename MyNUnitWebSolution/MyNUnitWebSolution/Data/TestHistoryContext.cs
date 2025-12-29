namespace MyNUnitWebSolution.Data;

using Microsoft.EntityFrameworkCore;

public class TestHistoryContext(DbContextOptions<TestHistoryContext> options) : DbContext(options)
{
    public DbSet<TestRun> TestRuns => this.Set<TestRun>();

    public DbSet<AssemblyRun> AssemblyRuns => this.Set<AssemblyRun>();

    public DbSet<TestOutputLine> TestOutputLines => this.Set<TestOutputLine>();
}
