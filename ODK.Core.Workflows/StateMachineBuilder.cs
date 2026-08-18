namespace ODK.Core.Workflows;

public sealed class StateMachineBuilder<TState, TTrigger, TContext>
    where TState : struct, Enum
    where TTrigger : struct, Enum
{
    private readonly string _name;
    private readonly List<Transition<TState, TTrigger, TContext>> _transitions = [];
    private TState _initialState;

    internal StateMachineBuilder(string name)
    {
        _name = name;
    }

    public StateMachineDefinition<TState, TTrigger, TContext> Build()
    {
        var definition = new StateMachineDefinition<TState, TTrigger, TContext>
        {
            InitialState = _initialState,
            Name = _name,
            Transitions = _transitions.ToArray()
        };

        var problems = Validate(definition);
        if (problems.Count > 0)
        {
            throw new StateMachineDefinitionException(
                $"{_name} is not a valid state machine:{Environment.NewLine}" +
                string.Join(Environment.NewLine, problems.Select(x => $"  - {x}")));
        }

        return definition;
    }

    public StateMachineBuilder<TState, TTrigger, TContext> StartingAt(TState state)
    {
        _initialState = state;
        return this;
    }

    public StateMachineBuilder<TState, TTrigger, TContext> Transition(
        TState from,
        TTrigger trigger,
        TState to,
        Action<TransitionBuilder<TContext>>? configure = null)
    {
        var builder = new TransitionBuilder<TContext>();
        configure?.Invoke(builder);

        _transitions.Add(new Transition<TState, TTrigger, TContext>
        {
            Description = builder.Description(),
            From = from,
            Guards = builder.Guards(),
            Steps = builder.Steps(),
            To = to,
            Trigger = trigger
        });

        return this;
    }

    private static IReadOnlyCollection<string> Validate(
        StateMachineDefinition<TState, TTrigger, TContext> definition)
    {
        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(definition.Name))
        {
            problems.Add("It has no name");
        }

        if (definition.Transitions.Count == 0)
        {
            problems.Add("It has no transitions");
        }

        // Zero is reserved for None on every enum here, so it is never a state a machine starts in.
        if (EqualityComparer<TState>.Default.Equals(definition.InitialState, default))
        {
            problems.Add("It has no initial state");
        }

        var wiredStates = definition.Transitions
            .SelectMany(x => new[] { x.From, x.To })
            .ToHashSet();
        foreach (var state in UsableValues<TState>().Where(x => !wiredStates.Contains(x)))
        {
            problems.Add($"State {state} is declared but never entered or left");
        }

        var wiredTriggers = definition.Transitions
            .Select(x => x.Trigger)
            .ToHashSet();
        foreach (var trigger in UsableValues<TTrigger>().Where(x => !wiredTriggers.Contains(x)))
        {
            problems.Add($"Trigger {trigger} is declared but never fired");
        }

        foreach (var transition in definition.Transitions)
        {
            problems.AddRange(Validate(transition));
        }

        var duplicates = definition.Transitions
            .GroupBy(x => $"{x.From}|{x.To}|{x.Label()}")
            .Where(x => x.Count() > 1);
        foreach (var duplicate in duplicates)
        {
            var first = duplicate.First();
            problems.Add($"{first.From} -> {first.To} on {first.Label()} is declared more than once");
        }

        return problems;
    }

    private static IReadOnlyCollection<string> Validate(Transition<TState, TTrigger, TContext> transition)
    {
        var problems = new List<string>();
        var edge = $"{transition.From} -> {transition.To} on {transition.Trigger}";

        foreach (var guard in transition.Guards.Where(x => string.IsNullOrWhiteSpace(x.Description)))
        {
            problems.Add($"{edge}: {guard.GetType().Name} has no description, so it cannot label the edge");
        }

        foreach (var step in transition.Steps)
        {
            if (string.IsNullOrWhiteSpace(step.Description))
            {
                problems.Add($"{edge}: {step.StepType.Name} has no description");
            }

            if (step.Kind == StepKind.None)
            {
                problems.Add($"{edge}: {step.StepType.Name} does not say what kind of step it is");
            }
        }

        /* The commit-before-external-effect rule, enforced rather than remembered: an irreversible effect
           taken while a write is still staged is an effect against state that can still roll back. An
           external effect with nothing pending is safe, which is why this tracks staged writes rather than
           simply requiring a commit first. */
        var staged = false;
        foreach (var step in transition.Steps)
        {
            switch (step.Kind)
            {
                case StepKind.Write:
                    staged = true;
                    break;
                case StepKind.Commit:
                    staged = false;
                    break;
                case StepKind.ExternalEffect when staged:
                    problems.Add(
                        $"{edge}: {step.StepType.Name} takes an external effect while a write is uncommitted");
                    break;
            }
        }

        return problems;
    }

    private static IReadOnlyCollection<T> UsableValues<T>()
        where T : struct, Enum
        => Enum.GetValues<T>()
            .Where(x => !EqualityComparer<T>.Default.Equals(x, default))
            .ToArray();
}
