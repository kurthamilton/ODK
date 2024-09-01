using Microsoft.AspNetCore.Html;

namespace ODK.Web.Razor.Models.Components;

public class TwoColLeftMenuViewModel
{
    public required Func<object?, IHtmlContent> BodyContentFunc { get; init; }

    public IHtmlContent? MenuContent { get; init; }

    public Func<object?, IHtmlContent>? MenuContentFunc { get; init; }
}
