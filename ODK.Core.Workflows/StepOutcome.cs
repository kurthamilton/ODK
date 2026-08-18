namespace ODK.Core.Workflows;

/// <summary>
/// The result of a single step. A failure stops the transition, leaving the steps after it unrun.
/// </summary>
public sealed class StepOutcome
{
    private StepOutcome(bool success, string? message)
    {
        Message = message;
        Success = success;
    }

    public string? Message { get; }

    public bool Success { get; }

    public static StepOutcome Continue() => new(true, null);

    public static StepOutcome Fail(string message) => new(false, message);
}
