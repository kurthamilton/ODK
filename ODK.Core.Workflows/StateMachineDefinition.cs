namespace ODK.Core.Workflows;

public sealed class StateMachineDefinition<TState, TTrigger, TContext> : IStateMachineDiagram
    where TState : struct, Enum
    where TTrigger : struct, Enum
{
    public required TState InitialState { get; init; }

    public required string Name { get; init; }

    /// <summary>
    /// Every step type the machine names, once each. A definition is the registry of its own steps, so a
    /// container learns them from here rather than from a list kept alongside.
    /// </summary>
    public IReadOnlyCollection<Type> StepTypes => Transitions
        .SelectMany(x => x.Steps)
        .Select(x => x.StepType)
        .Distinct()
        .ToArray();

    public required IReadOnlyCollection<Transition<TState, TTrigger, TContext>> Transitions { get; init; }

    public IReadOnlyCollection<Transition<TState, TTrigger, TContext>> From(TState state) => Transitions
        .Where(x => EqualityComparer<TState>.Default.Equals(x.From, state))
        .ToArray();

    public string ToMermaid() => MermaidExporter.ToStateDiagram(this);
}
