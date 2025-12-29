namespace MyNUnitWebSolution.Data;

public class TestOutputLine
{
    public int Id { get; init; }

    public int AssemblyRunId { get; init; }

    public AssemblyRun AssemblyRun { get; init; }

    public int Order { get; init; }

    public string Text { get; init; } = null!;
}