using Microsoft.AspNetCore.Html;
using ODK.Web.Common.Components;

namespace ODK.Web.Razor.Models.Login;

public class LoginBodyViewModel
{
    public IReadOnlyCollection<MenuItem>? Breadcrumbs { get; init; }

    public IHtmlContent? Content { get; set; }

    public Func<object?, IHtmlContent>? ContentFunc { get; set; }

    public required string Title { get; init; }
}