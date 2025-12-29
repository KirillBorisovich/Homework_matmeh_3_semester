namespace MyNUnitWebSolution.Data;

public class TestRun
{
    public int Id { get; init; }

    public DateTime StartedAt { get; init; }

    public DateTime FinishedAt { get; init; }

    public ICollection<AssemblyRun> Assemblies { get; init; } = new List<AssemblyRun>();
}