
namespace ODK.Data.Core.Deferred;

public class DefaultDeferredQueryMultiple<T> : IDeferredQueryMultiple<T>
{
    public async Task<IReadOnlyCollection<T>> Run() => await Task.FromResult(Array.Empty<T>());
}
