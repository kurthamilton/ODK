namespace ODK.Data.Core.Deferred;

public class DefaultDeferredQuerySingleOrDefault<T> : IDeferredQuerySingleOrDefault<T>
{
    public Task<T?> RunAsync() => Task.FromResult(default(T?));
}
