﻿using ODK.Core.Members;

namespace ODK.Services.Events.ViewModels;

public class EventInvitesAdminPageViewModel : EventAdminPageViewModelBase
{
    public required EventInvitesDto Invites { get; init; }    

    public required IReadOnlyCollection<Member> Members { get; init; }
}
