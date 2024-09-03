using Microsoft.AspNetCore.Html;
using ODK.Core.Chapters;

namespace ODK.Web.Razor.Models.Account;

public class SiteAccountLayoutViewModel
{    
    public required string Active { get; set; }

    public Chapter? Chapter { get; set; }

    public required Func<object?, IHtmlContent> Content { get; init; }
}
