using Microsoft.AspNetCore.Html;
using ODK.Web.Common.Components;

namespace ODK.Web.Razor.Models.Components
{
    public class BodyViewModel
    {
        public IReadOnlyCollection<MenuItem>? Breadcrumbs { get; set; }

        public IHtmlContent? Content { get; set; }

        public string? Title { get; set; }
    }
}
