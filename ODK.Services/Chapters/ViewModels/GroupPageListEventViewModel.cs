﻿using ODK.Core.Events;
using ODK.Core.Venues;

namespace ODK.Services.Chapters.ViewModels;

public class GroupPageListEventViewModel
{
    public required Event Event { get; init; }

    public required EventResponse? Response { get; init; }

    public required EventResponseSummaryDto? ResponseSummary { get; init; }

    public required Venue? Venue { get; init; }
}
