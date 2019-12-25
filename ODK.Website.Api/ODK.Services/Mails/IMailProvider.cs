using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Events;
using ODK.Core.Mail;
using ODK.Core.Members;
using ODK.Services.Events;

namespace ODK.Services.Mails
{
    public interface IMailProvider
    {
        string Name { get; }

        Task<string> CreateEventEmail(Event @event, Email email);

        Task<EventInvites> GetEventInvites(Event @event, EventEmail eventEmail);

        Task<IReadOnlyCollection<EventInvites>> GetInvites(Guid chapterId, IEnumerable<EventEmail> eventEmails);

        Task<bool> GetMemberOptIn(Member member);

        Task SendEmail(string to, string from, Email email, IDictionary<string, string> parameters = null);

        Task SendEventEmail(string id);

        Task SendTestEventEmail(string id, Member member);

        Task SynchroniseMembers();

        Task UpdateEventEmail(Event @event, Email email, string emailId);

        Task UpdateMemberOptIn(Member member, bool optIn);
    }
}
