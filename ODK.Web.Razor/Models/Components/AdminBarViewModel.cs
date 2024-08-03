using Microsoft.AspNetCore.Html;
using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Components;

public class AdminBarViewModel
{
    public required string AdminLinkText { get; init; }

    public required string AdminLink { get; init; }

    public required Chapter Chapter { get; init; }

    public required Member? CurrentMember { get; init; }

    public Func<object?, IHtmlContent>? Toolbar { get; init; }
}
