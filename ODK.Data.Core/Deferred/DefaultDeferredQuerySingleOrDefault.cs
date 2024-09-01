namespace ODK.Data.Core.Deferred;

public class DefaultDeferredQuerySingleOrDefault<T> : IDeferredQuerySingleOrDefault<T>
{
    public Task<T?> Run() => Task.FromResult(default(T?));
}
