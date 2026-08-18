namespace ODK.Core.Workflows;

public sealed class StateMachineRunner<TState, TTrigger, TContext>
    where TState : struct, Enum
    where TTrigger : struct, Enum
{
    private readonly StateMachineDefinition<TState, TTrigger, TContext> _definition;
    private readonly IStateResolver<TState, TContext> _stateResolver;
    private readonly IStepFactory<TContext> _stepFactory;

    public StateMachineRunner(
        StateMachineDefinition<TState, TTrigger, TContext> definition,
        IStateResolver<TState, TContext> stateResolver,
        IStepFactory<TContext> stepFactory)
    {
        _definition = definition;
        _stateResolver = stateResolver;
        _stepFactory = stepFactory;
    }

    public async Task<TransitionResult<TState>> Fire(
        TTrigger trigger, TContext context, CancellationToken cancellationToken = default)
    {
        var from = _stateResolver.Resolve(context);

        var permitted = _definition
            .From(from)
            .Where(x => EqualityComparer<TTrigger>.Default.Equals(x.Trigger, trigger) && x.IsPermitted(context))
            .ToArray();

        if (permitted.Length == 0)
        {
            return TransitionResult<TState>.Failed(from, $"{trigger} is not permitted from {from}");
        }

        /* Two satisfied guards on the same edge is a definition that does not say what should happen, and no
           query of the graph can catch it - which of them holds depends on the context. Picking one would make
           the diagram a description of something the code does not do. */
        if (permitted.Length > 1)
        {
            throw new StateMachineDefinitionException(
                $"{_definition.Name}: {trigger} from {from} matches more than one transition " +
                $"({string.Join("; ", permitted.Select(x => x.Label()))})");
        }

        var transition = permitted[0];

        foreach (var step in transition.Steps)
        {
            var outcome = await _stepFactory
                .Create(step.StepType)
                .Execute(context, cancellationToken);

            if (!outcome.Success)
            {
                return TransitionResult<TState>.Failed(from, outcome.Message ?? $"{step.Description} failed");
            }

            if (outcome.Complete)
            {
                break;
            }
        }

        return TransitionResult<TState>.Successful(from, transition.To);
    }
}
