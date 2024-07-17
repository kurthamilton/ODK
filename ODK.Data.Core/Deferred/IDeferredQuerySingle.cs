namespace ODK.Data.Core.Deferred;
/// <summary>
/// <inheritdoc cref="IDeferredQuery{TResult}"/>
/// <see cref="IDeferredQuerySingle{T}"/> is for returning a single object you expect to exist. Throws <see cref="NotFoundException"> otherwise.
/// </summary>
public interface IDeferredQuerySingle<T> : IDeferredQuery<T>
{
}
