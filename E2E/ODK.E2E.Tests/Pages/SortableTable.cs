using Microsoft.Playwright;

namespace ODK.E2E.Tests.Pages;

/// <summary>
/// A table whose column headings sort it, wired by <c>odk.lists.js</c> off <c>[data-sortable]</c>. A component
/// rather than a page: the same markup and the same script back the members, venues and payments admin tables,
/// so a test drives whichever list page it has arranged and asks about the order it shows.
/// </summary>
internal class SortableTable
{
    private readonly IPage _page;

    public SortableTable(IPage page)
    {
        _page = page;
    }

    /// <summary>Opens a list page and waits for its sortable table to arrive.</summary>
    public async Task Open(string url)
    {
        await _page.Navigate(url);
        await Table().WaitForAsync();
    }

    /// <summary>
    /// Where the row mentioning <paramref name="text"/> currently sits, counting from zero, or -1 when no row
    /// does. Matched on the row's own text rather than on a cell position, so it reads the way somebody
    /// scanning the table would and survives a column being added.
    /// </summary>
    public async Task<int> RowIndexOf(string text)
    {
        var rows = await Table().Locator("tbody tr").AllInnerTextsAsync();

        for (var i = 0; i < rows.Count; i++)
        {
            if (rows[i].Contains(text, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Clicks a column heading, which sorts the table by that column - and reverses it when it is already the
    /// column being sorted by. The script reorders the rows in the click handler, so there is nothing to wait
    /// for once the click has been dispatched.
    /// </summary>
    public Task SortBy(string heading) => Table()
        .Locator("thead th")
        .Filter(new() { HasText = heading })
        .ClickAsync();

    private ILocator Table() => _page.Locator("table[data-sortable]");
}
