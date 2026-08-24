using ODK.Web.Common.Routes;
using ODK.Web.Razor.Models.Components;

namespace ODK.Web.Razor.Models.Admin;

public class AdminLinkViewModel
{
    public IReadOnlyDictionary<string, string>? Attributes { get; init; }

    public string? Class { get; init; }

    public IconType? Icon { get; init; }

    /// <summary>
    /// Renders the link as its icon alone, with <see cref="Text"/> becoming its tooltip and its accessible
    /// name. For a toolbar; it needs an <see cref="Icon"/> to show.
    /// </summary>
    public bool IconOnly { get; init; }

    public string? QueryString { get; init; }

    public required GroupAdminRoute Route { get; init; }

    public required string Text { get; init; }

    public AdminLinkUnauthorizedBehaviour? UnauthorizedBehaviour { get; init; }
}
