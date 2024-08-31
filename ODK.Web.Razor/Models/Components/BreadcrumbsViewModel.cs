using ODK.Web.Common.Components;

namespace ODK.Web.Razor.Models.Components;

public class BreadcrumbsViewModel
{
    public required IReadOnlyCollection<MenuItem> Breadcrumbs { get; init; }
}
