﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Events;
using ODK.Core.Members;

namespace ODK.Services.Events
{
    public interface IEventService
    {
        Task<Event> GetEvent(Guid chapterId, Guid eventId);

        Task<IReadOnlyCollection<EventResponse>> GetEventResponses(Guid currentMemberId, Guid eventId);

        Task<IReadOnlyCollection<EventResponse>> GetEventResponses(Guid eventId);

        Task<IReadOnlyCollection<EventResponseViewModel>> GetEventResponseViewModels(Member member, Guid chapterId);

        Task<IReadOnlyCollection<EventResponseViewModel>> GetEventResponseViewModels(Member member, Guid chapterId, 
            DateTime? after);

        Task<IReadOnlyCollection<Event>> GetEvents(Guid currentMemberId, Guid chapterId);

        Task<IReadOnlyCollection<EventResponse>> GetMemberResponses(Guid memberId);

        Task<IReadOnlyCollection<Event>> GetPublicEvents(Guid chapterId);

        Task<ServiceResult> UpdateMemberResponse(Member member, Guid eventId, EventResponseType responseType);

        Task<EventResponse> UpdateMemberResponseOld(Guid memberId, Guid eventId, EventResponseType responseType);
    }
}