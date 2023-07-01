using ODK.Web.Common.Components;

namespace ODK.Web.Razor.Models.Components
{
    public class BreadcrumbsViewModel
    {
        public BreadcrumbsViewModel(IEnumerable<MenuItem> breadcrumbs)
        {
            Breadcrumbs = breadcrumbs.ToArray();
        }

        public IReadOnlyCollection<MenuItem> Breadcrumbs { get; }
    }
}
