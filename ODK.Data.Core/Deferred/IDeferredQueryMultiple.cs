namespace ODK.Data.Core.Deferred;
/// <summary>
/// <inheritdoc cref="IDeferredQuery{TResult}"/>
/// <see cref="IDeferredQueryMultiple{T}"/> is for returning a collection of objects.
/// </summary>
public interface IDeferredQueryMultiple<T> : IDeferredQuery<IReadOnlyCollection<T>>
{
}
