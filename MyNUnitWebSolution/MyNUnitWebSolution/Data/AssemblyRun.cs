namespace MyNUnitWebSolution.Data;

public class AssemblyRun
{
    public int Id { get; init; }

    public int TestRunId { get; init; }

    public TestRun TestRun { get; init; } = null!;

    public string AssemblyName { get; init; } = null!;

    public ICollection<TestOutputLine> Lines { get; init; } = new List<TestOutputLine>();
}