namespace ODK.Core.Workflows;

public sealed class TransitionResult<TState>
    where TState : struct, Enum
{
    private TransitionResult(bool success, TState from, TState to, string? message)
    {
        From = from;
        Message = message;
        Success = success;
        To = to;
    }

    public TState From { get; }

    public string? Message { get; }

    public bool Success { get; }

    /// <summary>The state arrived in. Equal to <see cref="From"/> when the transition failed.</summary>
    public TState To { get; }

    public static TransitionResult<TState> Failed(TState from, string message) => new(false, from, from, message);

    public static TransitionResult<TState> Successful(TState from, TState to) => new(true, from, to, null);
}
