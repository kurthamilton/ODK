using Microsoft.AspNetCore.Html;

namespace ODK.Web.Razor.Models.Components;

public class ToastViewModel
{
    public Func<object?, IHtmlContent>? Body { get; init; }

    public string? Class { get; init; }

    public int? Delay { get; init; }

    public Func<object?, IHtmlContent>? Header { get; init; }

    public bool KeepOpen { get; init; }
}