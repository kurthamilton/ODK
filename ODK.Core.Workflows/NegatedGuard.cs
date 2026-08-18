namespace ODK.Core.Workflows;

/// <summary>The inverse of another guard, so a condition and its opposite are one class rather than two.</summary>
public sealed class NegatedGuard<TContext> : IGuard<TContext>
{
    private readonly IGuard<TContext> _guard;

    public NegatedGuard(IGuard<TContext> guard)
    {
        _guard = guard;
    }

    public string Description => $"not {_guard.Description}";

    public bool IsSatisfied(TContext context) => !_guard.IsSatisfied(context);
}
