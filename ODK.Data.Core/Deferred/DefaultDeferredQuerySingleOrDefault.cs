namespace ODK.Data.Core.Deferred;

public static class DefaultDeferredQuerySingleOrDefault
{
    public static IDeferredQuerySingleOrDefault<T> For<T>(T? value)
        => new DefaultDeferredQuerySingleOrDefault<T>(value);
}

public class DefaultDeferredQuerySingleOrDefault<T> : IDeferredQuerySingleOrDefault<T>
{
    private readonly T? _value;

    public DefaultDeferredQuerySingleOrDefault()
        : this(default)
    {
    }

    public DefaultDeferredQuerySingleOrDefault(T? value)
    {
        _value = value;
    }

    public Task<T?> Run() => Task.FromResult(_value);    
}
