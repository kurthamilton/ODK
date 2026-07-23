namespace ODK.Web.Razor.Models.Components;

public class PaginationViewModel
{
    public required string AccessibilityLabel { get; init; }

    public required Func<int, string> GetPageUrl { get; init; }

    public required int Page { get; init; }

    public required int TotalPages { get; init; }
}
