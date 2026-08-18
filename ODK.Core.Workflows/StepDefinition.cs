namespace ODK.Core.Workflows;

/// <summary>
/// A step's place on a transition: which type runs, and the metadata copied from it when the
/// definition was built.
/// </summary>
public sealed class StepDefinition
{
    public required string Description { get; init; }

    public required StepKind Kind { get; init; }

    public required Type StepType { get; init; }
}
