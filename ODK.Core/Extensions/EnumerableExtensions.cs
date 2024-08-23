﻿namespace ODK.Core.Extensions;

public static class EnumerableExtensions
{
    public static int FindIndexOf<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        var count = source.Count();
        for (var i = 0; i < count; i++)
        {
            var item = source.ElementAt(i);
            if (predicate(item))
            {
                return i;
            }
        }

        return -1;
    }

    public static int IndexOf<T>(this IEnumerable<T> collection, T item)
        => collection.FindIndexOf(x => x?.Equals(item) == true);
}
