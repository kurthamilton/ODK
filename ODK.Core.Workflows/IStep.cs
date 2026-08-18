namespace ODK.Core.Workflows;

/// <summary>
/// One unit of work on a transition. The description and kind are static so a definition can carry
/// them without constructing the step, which is what lets a diagram be generated from types alone.
/// </summary>
public interface IStep<TContext>
{
    static abstract string Description { get; }

    static abstract StepKind Kind { get; }

    Task<StepOutcome> Execute(TContext context, CancellationToken cancellationToken);
}
