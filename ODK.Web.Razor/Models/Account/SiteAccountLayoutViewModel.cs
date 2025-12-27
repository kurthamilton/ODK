using Microsoft.AspNetCore.Html;
using ODK.Core.Chapters;

namespace ODK.Web.Razor.Models.Account;

public class SiteAccountLayoutViewModel
{
    public required string Active { get; init; }

    public Chapter? Chapter { get; init; }

    public required Func<object?, IHtmlContent> Content { get; init; }

    public required string Title { get; init; }
}
