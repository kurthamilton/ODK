using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Mail;
using ODK.Core.Members;
using ODK.Services.Events;

namespace ODK.Services.Mails
{
    public interface IMailProvider
    {
        Task<string> CreateEventEmail(ChapterEmailSettings emailSettings, Event @event, Chapter chapter, Email email);

        Task<EventInvites> GetEventInvites(Event @event, EventEmail eventEmail);

        Task<IReadOnlyCollection<EventInvites>> GetInvites(Guid chapterId, IEnumerable<EventEmail> eventEmails);

        Task<bool> GetMemberOptIn(Member member);

        Task SendEventEmail(ChapterEmailSettings emailSettings, string id);

        Task SendTestEventEmail(ChapterEmailSettings emailSettings, string id, Member member);

        Task SynchroniseMembers(ChapterEmailSettings emailSettings, Chapter chapter);

        Task UpdateEventEmail(ChapterEmailSettings emailSettings, Event @event, Chapter chapter, Email email, 
            string emailId);

        Task UpdateMemberOptIn(Member member, bool optIn);
    }
}
