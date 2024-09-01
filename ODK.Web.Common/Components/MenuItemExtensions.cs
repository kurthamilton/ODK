using System;
using System.Collections.Generic;

namespace ODK.Web.Common.Components;
public static class MenuItemExtensions
{
    public static MenuItem? Active(this IEnumerable<MenuItem> items, string? path)
    {
        if (path == null)
        {
            return null;
        }

        foreach (var item in items)
        {
            if (item.Link == null)
            {
                continue;
            }

            if (string.Equals(item.Link, path, StringComparison.InvariantCultureIgnoreCase))
            {
                return item;
            }

            if (item.Children == null)
            {
                continue;
            }

            var descendant = item.Children.Active(path);
            if (descendant != null)
            {
                return descendant;
            }
        }

        return null;
    }

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

    public static bool DescendantOf(this MenuItem descendant, MenuItem ancestor)
    {
        if (ancestor == descendant)
        {
            return true;
        }

        if (ancestor.Children == null)
        {
            return false;
        }

        foreach (var child in ancestor.Children)
        {
            if (descendant.DescendantOf(child))
            {
                return true;
            }
        }

        return false;
    }
}
