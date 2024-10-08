﻿using ODK.Core.Chapters;
using ODK.Core.Platforms;
using ODK.Services.Events.ViewModels;

namespace ODK.Web.Razor.Models.Events;

public class ListEventViewModel
{
    public required Chapter Chapter { get; init; }

    public required EventResponseViewModel Event { get; init; }

    public bool ForceDisplayYear { get; init; }

    public required PlatformType Platform { get; init; }

    public required TimeZoneInfo TimeZone { get; init; }
}
