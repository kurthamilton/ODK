namespace ODK.Web.Razor.Models.Components;

public class BreadcrumbsViewModel
{
    public required IReadOnlyCollection<MenuItem> Breadcrumbs { get; init; }
}
