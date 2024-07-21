using Microsoft.AspNetCore.Html;

namespace ODK.Web.Razor.Models.Components;

public class SectionViewModel
{
    public string? Class { get; set; }

    public Func<object?, IHtmlContent>? Content { get; set; }

    public bool Hero { get; set; }

    public string? Id { get; set; }

    public string? Theme { get; set; }

    public string? Title { get; set; }
}
