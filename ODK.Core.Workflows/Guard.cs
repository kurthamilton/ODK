namespace ODK.Core.Workflows;

public static class Guard
{
    public static IGuard<TContext> Not<TContext>(IGuard<TContext> guard) => new NegatedGuard<TContext>(guard);
}
