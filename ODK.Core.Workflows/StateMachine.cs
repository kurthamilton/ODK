namespace ODK.Core.Workflows;

public static class StateMachine
{
    public static StateMachineBuilder<TState, TTrigger, TContext> Define<TState, TTrigger, TContext>(string name)
        where TState : struct, Enum
        where TTrigger : struct, Enum
        => new(name);
}
