using System;
using System.Threading.Tasks;
using ODK.Core.Events;
using ODK.Core.Mail;
using ODK.Services.Events;

namespace ODK.Services.Mails
{
    public interface IMailProvider
    {
        Task<EventInvites> GetEventInvites(Event @event, EventEmail eventEmail);

        Task<string> SendEventEmail(Event @event, Email email);

        Task SynchroniseMembers(Guid chapterId);
    }
}
