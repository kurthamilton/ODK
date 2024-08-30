using System;
using System.Collections.Generic;
using ODK.Web.Common.Components;

namespace ODK.Web.Common.Extensions;
public static class MenuItemExtensions
{
    public static MenuItem? Closest(this IEnumerable<MenuItem> items, string? path)
    {
        if (path == null)
        {
            return null;
        }

        MenuItem? bestMatch = null;

        foreach (var item in items)
        {
            if (item.Link == null)
            {
                continue;
            }

            if (!path.StartsWith(item.Link, StringComparison.InvariantCultureIgnoreCase))
            {
                continue;
            }

            if (bestMatch == null)
            {
                bestMatch = item;
                continue;
            }
            
            if (item.Link.Length > bestMatch.Link?.Length)
            {
                bestMatch = item;
            }
        }

        if (bestMatch?.Children?.Count > 0)
        {
            var bestDescendantMatch = bestMatch.Children.Closest(path);
            if (bestDescendantMatch != null)
            {
                return bestDescendantMatch;
            }
        }

        return bestMatch;
    }
}
