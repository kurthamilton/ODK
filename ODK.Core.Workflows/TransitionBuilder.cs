namespace ODK.Core.Workflows;

public sealed class TransitionBuilder<TContext>
{
    private readonly List<IGuard<TContext>> _guards = [];
    private readonly List<StepDefinition> _steps = [];
    private string? _description;

    public TransitionBuilder<TContext> Describe(string description)
    {
        _description = description;
        return this;
    }

    /// <summary>
    /// Adds a step, taking its description and kind from the type itself so the two can never
    /// disagree.
    /// </summary>
    public TransitionBuilder<TContext> Then<TStep>()
        where TStep : IStep<TContext>
    {
        _steps.Add(new StepDefinition
        {
            Description = TStep.Description,
            Kind = TStep.Kind,
            StepType = typeof(TStep)
        });

        return this;
    }

    public TransitionBuilder<TContext> When(IGuard<TContext> guard)
    {
        _guards.Add(guard);
        return this;
    }

    internal string? Description() => _description;

    internal IReadOnlyCollection<IGuard<TContext>> Guards() => _guards.ToArray();

    internal IReadOnlyCollection<StepDefinition> Steps() => _steps.ToArray();
}
