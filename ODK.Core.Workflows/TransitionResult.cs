namespace ODK.Core.Workflows;

public sealed class TransitionResult<TState>
    where TState : struct, Enum
{
    private TransitionResult()
    {
    }

    public TState From { get; private init; }

    public string? Message { get; private init; }

    public bool Success { get; private init; }

    /// <summary>The state arrived in. Equal to <see cref="From"/> when the transition failed.</summary>
    public TState To { get; private init; }

    public static TransitionResult<TState> Failed(TState from, string message) => new()
    {
        From = from,
        Message = message,
        To = from
    };

    public static TransitionResult<TState> Successful(TState from, TState to) => new()
    {
        From = from,
        Success = true,
        To = to
    };
}
