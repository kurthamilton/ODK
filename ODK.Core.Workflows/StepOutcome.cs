namespace ODK.Core.Workflows;

/// <summary>
/// The result of a single step. Either of the two outcomes that are not <see cref="Continue"/> leaves the
/// steps after it unrun - the difference is whether the transition is reported as having succeeded.
/// </summary>
public sealed class StepOutcome
{
    private StepOutcome()
    {
    }

    /// <summary>Whether the transition is finished, with no further step to run.</summary>
    public bool Complete { get; private init; }

    public string? Message { get; private init; }

    public bool Success { get; private init; }

    /// <summary>Carry on to the next step.</summary>
    public static StepOutcome Continue() => new()
    {
        Success = true
    };

    /// <summary>Abandon the transition and report the failure.</summary>
    public static StepOutcome Fail(string message) => new()
    {
        Complete = true,
        Message = message
    };

    /// <summary>
    /// The transition is already done: stop here and report success. For a step that finds the work it was
    /// about to do has been done already - a resubmitted form, a webhook arriving twice - where the steps
    /// after it would repeat an effect rather than complete one.
    /// </summary>
    public static StepOutcome Stop() => new()
    {
        Complete = true,
        Success = true
    };
}
