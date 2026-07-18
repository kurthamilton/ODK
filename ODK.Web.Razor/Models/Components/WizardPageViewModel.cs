using Microsoft.AspNetCore.Html;

namespace ODK.Web.Razor.Models.Components;

public class WizardPageViewModel
{
    public string? ButtonSubmit { get; init; }

    public string? ButtonText { get; init; }

    public required Func<object?, IHtmlContent> ContentFunc { get; init; }

    public Func<object?, IHtmlContent>? FooterFunc { get; init; }

    public string? Url { get; init; }

    public bool Selected { get; init; }

    public string? Title { get; init; }
}
