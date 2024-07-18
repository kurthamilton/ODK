namespace ODK.Data.Core.Deferred;
public static class DeferreredDefaults
{
    public static IDeferredQueryMultiple<T> Multiple<T>() => new DefaultMultipleDeferredQuery<T>();

    public static IDeferredQuerySingle<T> Single<T>(T value) => new DefaultSingleDeferredQuery<T>(value);

    public static IDeferredQuerySingleOrDefault<T> SingleOrDefault<T>() => new DefaultSingleOrDefaultDeferredQuery<T>(default);
}
