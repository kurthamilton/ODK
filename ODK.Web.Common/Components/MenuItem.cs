using System.Collections.Generic;

namespace ODK.Web.Common.Components;

public class MenuItem
{
    public bool Active { get; init; }

    public bool ActiveIsExactMatch { get; init; }

    public IReadOnlyCollection<MenuItem>? Children { get; init; }

    public string? ExternalLink { get; init; }

    public bool Hidden { get; init; }

    public IconType? Icon { get; init; }

    public string? Link { get; init; }
    
    public string? Text { get; init; }

    public IEnumerable<MenuItem> Expand()
    {
        yield return this;

        if (Children != null)
        {
            foreach (var child in Children)
            {
                foreach (var expanded in child.Expand())
                {
                    yield return expanded;
                }
            }
        }
    }
}
