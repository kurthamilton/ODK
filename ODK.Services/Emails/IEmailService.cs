using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Events;
using ODK.Core.Members;

namespace ODK.Services.Emails;

public interface IEmailService
{
    Task AddEvent(string externalId, string eventName);

    /// <summary>
    /// Resolves an email's wording without queuing it, for a preview. The send path renders through this
    /// too, so the two cannot disagree.
    /// </summary>
    Task<RenderedEmail> RenderEmail(IServiceRequest request, RenderEmailOptions options);

    Task SendBulkEmail(
        IChapterServiceRequest request,
        IEnumerable<Member> to,
        EmailType type,
        IEmailParameters? parameters);

    /* The two sends carrying their own subject and body - this one and the SendEmail below it - have no
       email row to say who they are written for, so each states its own recipient type, which is what
       {title} resolves through.

       They exist for copy an admin typed: a bulk email to a group's members, and a referral campaign's
       own wording. Every notification the app itself sends reads from the Emails table - reach for the
       EmailType overloads and add a row, not for these. */
    Task SendBulkEmail(
        IChapterServiceRequest request,
        IEnumerable<Member> to,
        string subject,
        string body,
        EmailRecipientType recipientType);

    Task SendEventCommentEmail(
        IServiceRequest request,
        Chapter chapter,
        Member? replyToMember,
        EventComment comment,
        IEmailParameters? parameters);

    Task<ServiceResult> SendEmail(
        IServiceRequest request,
        Chapter? chapter,
        EmailAddressee to,
        EmailType type,
        IEmailParameters? parameters);

    Task<ServiceResult> SendEmail(
        IServiceRequest request,
        Chapter? chapter,
        IEnumerable<EmailAddressee> to,
        EmailType type,
        IEmailParameters? parameters);

    Task<ServiceResult> SendEmail(
        IServiceRequest request,
        Chapter? chapter,
        IEnumerable<EmailAddressee> to,
        string subject,
        string body,
        EmailRecipientType recipientType,
        IEmailParameters? parameters);
}