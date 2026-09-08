using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Data.EntityFramework.Queries;

internal static class ChapterQueries
{
    extension(IQueryable<Chapter> query)
    {
        internal IQueryable<Chapter> Published() => query.Where(x => x.PublishedUtc != null);

        /// <summary>
        /// The chapters a platform shows. This is a policy rather than a filter on a column: what a platform
        /// shows is not the same question as what it owns, and only one of the two platforms answers them alike.
        /// For "every chapter, whatever owns it", take an unfiltered query - <c>ChapterRepository.Query()</c> -
        /// rather than naming a platform here.
        /// </summary>
        internal IQueryable<Chapter> VisibleOn(PlatformType platform, bool includeUnpublished)
            => platform switch
            {
                /* Drunken Knitwits shows its own chapters and no others, published or not: an unpublished or
                   unapproved one carries a RedirectUrl by convention and sends a visitor somewhere useful. Do
                   not add the published gate here - it would hide chapters Drunken Knitwits expects to serve. */
                PlatformType.DrunkenKnitwits => query
                    .Where(x => x.Platform == PlatformType.DrunkenKnitwits),

                // Group Squirrel lists every platform's groups, its own and Drunken Knitwits' alike, which is
                // what makes a Drunken Knitwits group reachable there. Published only, unless asked otherwise.
                _ => includeUnpublished
                    ? query
                    : query.Published()
            };
    }
}
