using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Members;
using ODK.Data.Core;
using ODK.Services.Exceptions;

namespace ODK.Services.Emails;

public class EmailService : IEmailService
{
    private readonly IMailProvider _mailProvider;
    private readonly IUnitOfWork _unitOfWork;

    public EmailService(IUnitOfWork unitOfWork, IMailProvider mailProvider)
    {
        _mailProvider = mailProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task SendBulkEmail(Guid currentMemberId, Chapter chapter, IEnumerable<Member> to, EmailType type, 
        IDictionary<string, string> parameters)
    {
        var (from, chapterEmail, email) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByMemberId(currentMemberId, chapter.Id),
            x => x.ChapterEmailRepository.GetByChapterId(chapter.Id, type),
            x => x.EmailRepository.GetByType(type));

        email = GetEmail(chapterEmail?.ToEmail() ?? email, parameters);

        await _mailProvider.SendBulkEmail(chapter, to.Select(x => x.GetEmailAddressee()), email.Subject, email.HtmlContent, from);
    }

    public async Task SendBulkEmail(Guid currentMemberId, Chapter chapter, IEnumerable<Member> to, string subject, string body)
    {
        var from = await _unitOfWork.ChapterAdminMemberRepository.GetByMemberId(currentMemberId, chapter.Id).RunAsync();
        if (from == null)
        {
            throw new OdkNotAuthorizedException();
        }

        await _mailProvider.SendBulkEmail(chapter, to.Select(x => x.GetEmailAddressee()), subject, body, from);
    }

    public async Task SendContactEmail(Chapter chapter, string from, string message, 
        IDictionary<string, string> parameters)
    {
        var type = EmailType.ContactRequest;

        var (chapterAdminMembers, adminMembers, email, chapterEmail) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapter.Id),
            x => x.MemberRepository.GetAdminMembersByChapterId(chapter.Id),
            x => x.EmailRepository.GetByType(type),
            x => x.ChapterEmailRepository.GetByChapterId(chapter.Id, type));

        email = GetEmail(chapterEmail?.ToEmail() ?? email, parameters);

        var to = GetAddressees(chapterAdminMembers.Where(x => x.ReceiveContactEmails), adminMembers);

        await _mailProvider.SendBulkEmail(chapter, to, email.Subject, email.HtmlContent, false);
    }

    public async Task<ServiceResult> SendEmail(Chapter chapter, EmailAddressee to, EmailType type, 
        IDictionary<string, string> parameters)
    {
        var (email, chapterEmail) = await _unitOfWork.RunAsync(
            x => x.EmailRepository.GetByType(type),
            x => x.ChapterEmailRepository.GetByChapterId(chapter.Id, type));

        email = GetEmail(chapterEmail?.ToEmail() ?? email, parameters);

        return await _mailProvider.SendEmail(chapter, to, email.Subject, email.HtmlContent);
    }

    public async Task<ServiceResult> SendMemberEmail(Guid currentMemberId, Guid memberId, string subject, string body)
    {
        var to = await _unitOfWork.MemberRepository.GetById(memberId).RunAsync();

        var (from, chapter) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByMemberId(currentMemberId, to.ChapterId),
            x => x.ChapterRepository.GetById(to.ChapterId));

        return await _mailProvider.SendEmail(chapter!, to.GetEmailAddressee(), subject, body, from);
    }

    public async Task SendNewMemberAdminEmail(Chapter chapter, Member member, 
        IDictionary<string, string> parameters)
    {
        var type = EmailType.NewMemberAdmin;

        var (chapterAdminMembers, adminMembers, email, chapterEmail) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapter.Id),
            x => x.MemberRepository.GetAdminMembersByChapterId(chapter.Id),
            x => x.EmailRepository.GetByType(type),
            x => x.ChapterEmailRepository.GetByChapterId(chapter.Id, type));

        email = GetEmail(chapterEmail?.ToEmail() ?? email, parameters);

        var to = GetAddressees(chapterAdminMembers.Where(x => x.SendNewMemberEmails), adminMembers);
        
        await _mailProvider.SendBulkEmail(chapter, to, email.Subject, email.HtmlContent, false);
    }

    private static IEnumerable<EmailAddressee> GetAddressees(IEnumerable<ChapterAdminMember> adminMembers, IEnumerable<Member> members)
    {
        var memberDictionary = members.ToDictionary(x => x.Id);

        foreach (var adminMember in adminMembers)
        {
            var member = memberDictionary[adminMember.MemberId];
            var address = !string.IsNullOrEmpty(adminMember.AdminEmailAddress)
                ? adminMember.AdminEmailAddress
                : member.EmailAddress;
            yield return new EmailAddressee(address, member.FullName);
        }
    }

    private static Email GetEmail(Email email, IDictionary<string, string> parameters)
    {
        return email.Interpolate(parameters);
    }
}
