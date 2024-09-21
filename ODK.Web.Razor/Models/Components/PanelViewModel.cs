﻿using Microsoft.AspNetCore.Html;

namespace ODK.Web.Razor.Models.Components;

public class PanelViewModel
{
    public IHtmlContent? BodyContent { get; init; }

    public Func<object?, IHtmlContent>? BodyContentFunc { get; init; }    

    public required Func<object?, IHtmlContent> TitleContentFunc { get; init; }    

    public Func<object?, IHtmlContent>? TitleEndContentFunc { get; init; }
}
