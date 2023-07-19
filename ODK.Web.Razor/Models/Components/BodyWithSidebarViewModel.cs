using Microsoft.AspNetCore.Html;

namespace ODK.Web.Razor.Models.Components
{
    public class BodyWithSidebarViewModel : BodyViewModel
    {
        public IHtmlContent? SidebarContent { get; set; }
    }
}
