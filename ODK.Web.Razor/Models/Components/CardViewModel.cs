using Microsoft.AspNetCore.Html;

namespace ODK.Web.Razor.Models.Components;

public class CardViewModel
{
    public Func<object?, IHtmlContent>? BodyContentFunc { get; init; }

    public Func<object?, IHtmlContent>? FooterContentFunc { get; init; }

    public Func<object?, IHtmlContent>? HeaderContentFunc { get; init; }
}
