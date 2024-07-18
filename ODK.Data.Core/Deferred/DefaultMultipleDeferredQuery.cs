namespace ODK.Data.Core.Deferred;
public class DefaultMultipleDeferredQuery<T> : IDeferredQueryMultiple<T>
{
    private readonly IReadOnlyCollection<T> _emptyCollection = [];

    public Task<IReadOnlyCollection<T>> RunAsync() => Task.FromResult(_emptyCollection);
}
