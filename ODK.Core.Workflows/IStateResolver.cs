namespace ODK.Core.Workflows;

/// <summary>
/// Derives the current state from the domain. State is never stored, so this is the only thing that
/// decides what state something is in.
/// </summary>
public interface IStateResolver<TState, TContext>
    where TState : struct, Enum
{
    TState Resolve(TContext context);
}
