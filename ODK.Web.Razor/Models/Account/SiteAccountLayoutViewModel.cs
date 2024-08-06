using Microsoft.AspNetCore.Html;

namespace ODK.Web.Razor.Models.Account;

public class SiteAccountLayoutViewModel
{    
    public required string Active { get; set; }

    public string? ChapterName { get; set; }

    public required Func<object?, IHtmlContent> Content { get; init; }
}
