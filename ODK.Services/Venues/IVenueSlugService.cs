using ODK.Core.Venues;
using ODK.Services.Places;

namespace ODK.Services.Venues;

/// <summary>
/// How a place becomes a venue's slug. Two things set a venue's details from a place - creating one and
/// backfilling one that predates the lookup - and a slug the two built differently would be a venue
/// reachable by an address its own rules would not have given it.
/// </summary>
public interface IVenueSlugService
{
    /// <summary>
    /// A slug unique across the site. <paramref name="candidates"/> is every venue whose slug starts with
    /// <paramref name="slugBase"/>, which is the whole set a version of it could collide with.
    /// </summary>
    string CreateSlug(string slugBase, IReadOnlyCollection<Venue> candidates);

    /// <summary>
    /// The slug a place takes before versioning, and so the prefix that finds every slug it could collide
    /// with - "The Oak" in Sheffield gives <c>the-oak-sheffield</c>.
    /// </summary>
    string SlugBase(Place place);
}
