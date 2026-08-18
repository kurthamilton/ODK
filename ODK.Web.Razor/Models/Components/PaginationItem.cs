namespace ODK.Web.Razor.Models.Components;

/// <summary>
/// One entry in a rendered pagination: either a page, or the run of pages an ellipsis stands for.
/// </summary>
/// <remarks>
/// A gap carries the run it hides rather than just the fact of hiding one, so the ellipsis can offer to go
/// somewhere inside it. Without that the pages between two links are unreachable without guessing at a URL.
/// </remarks>
public sealed class PaginationItem
{
    private PaginationItem()
    {
    }

    /// <summary>The first page this entry stands for; the page itself where it is not a gap.</summary>
    public int From { get; private init; }

    public bool IsGap => PageNumber == null;

    /// <summary>
    /// The page a gap's control opens on: the end of the run nearest the page being viewed. Clicking the
    /// ellipsis beside where you are most often means "a little further", so the near end is the better
    /// guess than either the far end or the middle.
    /// </summary>
    public int NearestPage { get; private init; }

    /// <summary>The page to link to, or null where this entry is a gap.</summary>
    public int? PageNumber { get; private init; }

    /// <summary>The last page this entry stands for; the page itself where it is not a gap.</summary>
    public int To { get; private init; }

    public static PaginationItem ForGap(int from, int to, int currentPage) => new()
    {
        From = from,
        NearestPage = to < currentPage ? to : from,
        To = to
    };

    public static PaginationItem ForPage(int page) => new()
    {
        From = page,
        NearestPage = page,
        PageNumber = page,
        To = page
    };
}
