namespace ODK.Core.Workflows;

public sealed class Transition<TState, TTrigger, TContext>
    where TState : struct, Enum
    where TTrigger : struct, Enum
{
    /// <summary>Overrides the label the edge would otherwise compose from its trigger and guards.</summary>
    public string? Description { get; init; }

    public required TState From { get; init; }

    public required IReadOnlyCollection<IGuard<TContext>> Guards { get; init; }

    public required IReadOnlyCollection<StepDefinition> Steps { get; init; }

    public required TState To { get; init; }

    public required TTrigger Trigger { get; init; }

    public bool IsPermitted(TContext context) => Guards.All(x => x.IsSatisfied(context));

    /// <summary>How the edge reads, on a diagram and in a validation message.</summary>
    public string Label()
    {
        if (!string.IsNullOrWhiteSpace(Description))
        {
            return Description;
        }

        return Guards.Count > 0
            ? $"{Trigger} [{string.Join(", ", Guards.Select(x => x.Description))}]"
            : $"{Trigger}";
    }
}
