using ODK.Core.Utils;
using ODK.Core.Venues;
using ODK.Services.Places;

namespace ODK.Services.Venues;

/// <inheritdoc cref="IVenueSlugService"/>
public class VenueSlugService : IVenueSlugService
{
    /// <remarks>
    /// Compared case-insensitively to match SQL Server's default collation, so the slugs stay unique under
    /// the unique index on Slug.
    /// </remarks>
    public string CreateSlug(string slugBase, IReadOnlyCollection<Venue> candidates)
    {
        var taken = candidates
            .Select(x => x.Slug)
            .Where(x => !string.IsNullOrEmpty(x))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // slugBase is already a slug and never empty, so this always produces one.
        return UrlUtils.SlugifyUnique(slugBase, taken, Venue.SlugMaxLength)!;
    }

    /// <remarks>
    /// A place whose name and town have nothing sluggable between them falls back to a generic slug that
    /// the versioning then keeps unique.
    /// </remarks>
    public string SlugBase(Place place)
    {
        var source = !string.IsNullOrEmpty(place.Locality)
            ? $"{place.Name} {place.Locality}"
            : place.Name;

        return UrlUtils.SlugBase(source, Venue.SlugMaxLength) ?? Venue.SlugFallback;
    }
}
