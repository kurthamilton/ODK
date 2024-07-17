namespace ODK.Data.Core.Deferred;
/// <summary>
/// <inheritdoc cref="IDeferredQuery{TResult}"/>
/// <see cref="IDeferredQuerySingleOrDefault{T}"/> is for returning a single object.
/// </summary>
public interface IDeferredQuerySingleOrDefault<T> : IDeferredQuery<T?>
{
}
