using Microsoft.AspNetCore.Mvc.Rendering;

namespace ODK.Web.Razor.Models.Components;

public class ExpandedListBoxViewModel
{
    public required string Id { get; init; }

    public required IReadOnlyCollection<SelectListItem> Items { get; init; }
}
