using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Emails;
using ODK.Core.Members;
using ODK.Services.Events;

namespace ODK.Services.Emails
{
    public interface IMailProvider
    {
        string Name { get; }

        ChapterEmailProviderSettings Settings { get; }

        Task<string> CreateEventEmail(Event @event, Email email);

        Task<Email> GetEmailWithLayout(Chapter chapter, Email email);

        Task<EventInvites> GetEventInvites(Event @event, EventEmail eventEmail);

        Task<IReadOnlyCollection<EventInvites>> GetInvites(Guid chapterId, IEnumerable<EventEmail> eventEmails);

        Task<bool> GetMemberOptIn(Member member);

        Task SendEmail(ChapterAdminMember from, string to, string subject, string body);

        Task SendEmail(ChapterAdminMember from, IEnumerable<string> to, string subject, string body);

        Task SendEmail(ChapterAdminMember from, string to, Email email, IDictionary<string, string> parameters = null);

        Task SendEmail(ChapterAdminMember from, IEnumerable<string> to, Email email, IDictionary<string, string> parameters = null);

        Task<IReadOnlyCollection<Member>> SendEventEmail(Event @event, EventEmail eventEmail);

        Task SendTestEventEmail(string id, Member member);

        Task SynchroniseMembers();

        Task UpdateEventEmail(Event @event, Email email, string emailId);

        Task UpdateMemberEmailAddress(Member member, string newEmailAddress);

        Task UpdateMemberOptIn(Member member, bool optIn);
    }
}
