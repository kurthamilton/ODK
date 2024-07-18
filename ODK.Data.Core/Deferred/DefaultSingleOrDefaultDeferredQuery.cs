namespace ODK.Data.Core.Deferred;
public class DefaultSingleOrDefaultDeferredQuery<T> : IDeferredQuerySingleOrDefault<T>
{
    private readonly T? _value;

    public DefaultSingleOrDefaultDeferredQuery(T? value)
    {
        _value = value;
    }

    public Task<T?> RunAsync() => Task.FromResult(_value);
}
