using Microsoft.AspNetCore.Html;

namespace ODK.Web.Razor.Models.Components;

public class TwoColLeftMenuViewModel
{
    public required Func<object?, IHtmlContent> BodyContentFunc { get; init; }

    public required Func<object?, IHtmlContent> MenuContentFunc { get; init; }
}
