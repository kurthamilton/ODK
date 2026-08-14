namespace ODK.Web.Razor.Models.Components;

public class PaginationViewModel
{
    /// <summary>How many pages are shown either side of the current one.</summary>
    private const int Window = 1;

    public required string AccessibilityLabel { get; init; }

    public required Func<int, string> GetPageUrl { get; init; }

    public required int Page { get; init; }

    public required int TotalPages { get; init; }

    /// <summary>
    /// The pages to render, in order, with null where a run of them is replaced by an ellipsis. Always
    /// carries the first page, the last page, and the current page with <see cref="Window"/> either side.
    /// </summary>
    /// <remarks>
    /// Bounded on purpose. Rendering every page made the component wider than a phone screen once a list ran
    /// to dozens of pages, and an element wider than the viewport takes the rest of the page with it: mobile
    /// browsers widen the layout viewport to fit it, which is what a fixed-position element anchors to, so an
    /// offcanvas drawer ends up hanging off the far side of the screen.
    /// </remarks>
    public IReadOnlyCollection<int?> VisiblePages
    {
        get
        {
            var pages = new List<int?>();
            var previous = 0;

            foreach (var page in Enumerable.Range(1, TotalPages).Where(Include))
            {
                var gap = page - previous - 1;

                /* A gap of one page is rendered as that page rather than an ellipsis standing in for it -
                   the ellipsis is no shorter than the single number it would hide, and hides a page that
                   would otherwise be one click away. */
                if (gap == 1)
                {
                    pages.Add(page - 1);
                }
                else if (gap > 1)
                {
                    pages.Add(null);
                }

                pages.Add(page);
                previous = page;
            }

            return pages;
        }
    }

    private bool Include(int page)
        => page == 1 || page == TotalPages || Math.Abs(page - Page) <= Window;
}
