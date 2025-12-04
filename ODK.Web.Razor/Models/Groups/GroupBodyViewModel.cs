using Microsoft.AspNetCore.Html;
using ODK.Services.Chapters.ViewModels;
using ODK.Web.Common.Components;

namespace ODK.Web.Razor.Models.Groups;

public class GroupBodyViewModel
{
    public IReadOnlyCollection<MenuItem>? Breadcrumbs { get; init; }

    public required IHtmlContent Content { get; init; }

    public required GroupPageViewModel Group { get; init; }

    public required string Title { get; init; }
}
