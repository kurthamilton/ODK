﻿using Microsoft.AspNetCore.Html;
using ODK.Web.Common.Components;

namespace ODK.Web.Razor.Models.Admin;

public class AdminBodyViewModel
{
    public IReadOnlyCollection<MenuItem>? Breadcrumbs { get; init; }

    public required Func<object?, IHtmlContent> ContentFunc { get; init; }

    public string? Title { get; init; }
}
