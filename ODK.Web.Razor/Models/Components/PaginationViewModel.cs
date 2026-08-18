using System.Globalization;
namespace ODK.Web.Razor.Models.Components;

public class PaginationViewModel
{
    /// <summary>How many pages are shown either side of the current one.</summary>
    private const int Window = 1;

    /// <summary>
    /// How many page numbers the component offers: the first, the last, and the window around the current
    /// page. The same number wherever the current page sits, which is what <see cref="Run"/> is for.
    /// </summary>
    private const int Quota = (Window * 2) + 3;

    /// <summary>
    /// Stands in for a page number while <see cref="PageUrlTemplate"/> is built. No pagination reaches it,
    /// and a URL that happened to contain the same digits would have to contain all ten of them in order.
    /// </summary>
    private const int PlaceholderPage = int.MaxValue;

    public required string AccessibilityLabel { get; init; }

    public required Func<int, string> GetPageUrl { get; init; }

    public required int Page { get; init; }

    public required int TotalPages { get; init; }

    /// <summary>
    /// The page URL with <c>{page}</c> where the number goes, for the go-to-page control to fill in. The
    /// pages a gap stands for cannot all be rendered as links, so the browser builds one.
    /// </summary>
    /// <remarks>
    /// Taken by asking <see cref="GetPageUrl"/> for a page number no pagination will ever hold and swapping
    /// it out, which keeps this working whatever shape a caller's URLs take - a query parameter here, a route
    /// segment elsewhere - without every caller having to supply a second builder.
    /// </remarks>
    public string PageUrlTemplate => GetPageUrl(PlaceholderPage)
        .Replace(PlaceholderPage.ToString(CultureInfo.InvariantCulture), "{page}");

    /// <summary>
    /// The entries to render, in order: the first page, the last page, the current page with
    /// <see cref="Window"/> either side, and a gap standing for each run left out.
    /// </summary>
    /// <remarks>
    /// Bounded on purpose. Rendering every page made the component wider than a phone screen once a list ran
    /// to dozens of pages, and an element wider than the viewport takes the rest of the page with it: mobile
    /// browsers widen the layout viewport to fit it, which is what a fixed-position element anchors to, so an
    /// offcanvas drawer ends up hanging off the far side of the screen.
    /// </remarks>
    public IReadOnlyCollection<PaginationItem> VisiblePages
    {
        get
        {
            var (from, to) = Run();

            var pages = new List<PaginationItem>();
            var previous = 0;

            foreach (var page in Enumerable.Range(1, TotalPages)
                .Where(x => x == 1 || x == TotalPages || (x >= from && x <= to)))
            {
                var gap = page - previous - 1;

                /* A gap of one page is rendered as that page rather than an ellipsis standing in for it -
                   the ellipsis is no shorter than the single number it would hide, and hides a page that
                   would otherwise be one click away. */
                if (gap == 1)
                {
                    pages.Add(PaginationItem.ForPage(page - 1));
                }
                else if (gap > 1)
                {
                    pages.Add(PaginationItem.ForGap(previous + 1, page - 1, Page));
                }

                pages.Add(PaginationItem.ForPage(page));
                previous = page;
            }

            return pages;
        }
    }

    /// <summary>
    /// The run of pages shown around the current one, as an inclusive range.
    /// </summary>
    /// <remarks>
    /// The run offers the same number of links wherever the current page sits. Centred on the current page it
    /// is <see cref="Window"/> either side, and the first and last pages are added to it. Against either end
    /// there is nowhere to put half of it, so rather than losing those links it runs from the end instead and
    /// takes one more - absorbing the first or last page, which it would otherwise sit beside. Page 1 of 16
    /// therefore offers as many pages as page 8 does, instead of a third of them.
    /// </remarks>
    private (int From, int To) Run()
    {
        var atStart = Page - Window <= 1;
        var atEnd = Page + Window >= TotalPages;

        if (atStart && atEnd)
        {
            return (1, TotalPages);
        }

        if (atStart)
        {
            return (1, Math.Min(TotalPages, Quota - 1));
        }

        if (atEnd)
        {
            return (Math.Max(1, TotalPages - Quota + 2), TotalPages);
        }

        return (Page - Window, Page + Window);
    }
}
