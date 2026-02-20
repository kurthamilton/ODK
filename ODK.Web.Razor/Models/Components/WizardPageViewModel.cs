using Microsoft.AspNetCore.Html;

namespace ODK.Web.Razor.Models.Components;

public class WizardPageViewModel
{
    public required Func<object?, IHtmlContent> ContentFunc { get; init; }

    public Func<object?, IHtmlContent>? FooterFunc { get; init; }

    public string? Title { get; init; }
}
