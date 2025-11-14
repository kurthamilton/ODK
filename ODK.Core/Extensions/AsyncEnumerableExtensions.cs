namespace ODK.Core.Extensions;

public static class AsyncEnumerableExtensions
{
    public static async Task<IReadOnlyCollection<T>> All<T>(this IAsyncEnumerable<T> source)
    {
        var values = new List<T>();

        await foreach (var item in source)
        {
            values.Add(item);
        }

        return values;
    }
}
