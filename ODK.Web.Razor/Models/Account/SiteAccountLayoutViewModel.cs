using Microsoft.AspNetCore.Html;

namespace ODK.Web.Razor.Models.Account;

public class SiteAccountLayoutViewModel
{
    public required string Active { get; init; }

    public required Func<object?, IHtmlContent> Content { get; init; }

    public required string Title { get; init; }
}
