using ODK.Core;
using ODK.Core.Emails;
using ODK.Data.Core;
using ODK.Services.Members;

namespace ODK.Services.Emails;

public class EmailAdminService : OdkAdminServiceBase, IEmailAdminService
{
    private readonly IMemberEmailService _memberEmailService;
    private readonly IUnitOfWork _unitOfWork;

    public EmailAdminService(
        IUnitOfWork unitOfWork,
        IMemberEmailService memberEmailService)
        : base(unitOfWork)
    {
        _memberEmailService = memberEmailService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> DeleteChapterEmail(MemberChapterServiceRequest request, EmailType type)
    {
        var chapterEmail = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterEmailRepository.GetByChapterId(request.ChapterId, type));

        OdkAssertions.Exists(chapterEmail);

        _unitOfWork.ChapterEmailRepository.Delete(chapterEmail);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ChapterEmail> GetChapterEmail(MemberChapterServiceRequest request, EmailType type)
    {
        var (chapterEmail, siteEmail) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterEmailRepository.GetByChapterId(request.ChapterId, type),
            x => x.EmailRepository.GetByType(type));

        if (chapterEmail != null)
        {
            return chapterEmail;
        }

        return new ChapterEmail
        {
            ChapterId = request.ChapterId,
            HtmlContent = siteEmail.HtmlContent,
            Subject = siteEmail.Subject,
            Type = siteEmail.Type
        };
    }

    public async Task<IReadOnlyCollection<ChapterEmail>> GetChapterEmails(MemberChapterServiceRequest request)
    {
        var (chapterEmails, siteEmails) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterEmailRepository.GetByChapterId(request.ChapterId),
            x => x.EmailRepository.GetAll());

        var chapterEmailDictionary = chapterEmails.ToDictionary(x => x.Type);

        var emails = new List<ChapterEmail>();

        foreach (var siteEmail in siteEmails.OrderBy(x => x.Type))
        {
            if (!siteEmail.Overridable)
            {
                continue;
            }

            if (chapterEmailDictionary.TryGetValue(siteEmail.Type, out var chapterEmail))
            {
                emails.Add(chapterEmail);
            }
            else
            {
                emails.Add(new ChapterEmail
                {
                    ChapterId = request.ChapterId,
                    HtmlContent = siteEmail.HtmlContent,
                    Subject = siteEmail.Subject,
                    Type = siteEmail.Type
                });
            }
        }

        return emails;
    }

    public async Task<Email> GetEmail(Guid currentMemberId, EmailType type)
    {
        return await GetSiteAdminRestrictedContent(currentMemberId,
            x => x.EmailRepository.GetByType(type));
    }

    public async Task<IReadOnlyCollection<Email>> GetEmails(Guid currentMemberId)
    {
        return await GetSiteAdminRestrictedContent(currentMemberId,
            x => x.EmailRepository.GetAll());
    }

    public async Task<ServiceResult> SendTestEmail(MemberChapterServiceRequest request, EmailType type)
    {
        var (chapterId, currentMemberId) = (request.ChapterId, request.CurrentMemberId);

        var (chapter, currentMember) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(chapterId),
            x => x.MemberRepository.GetById(currentMemberId));

        return await _memberEmailService.SendTestEmail(request, chapter, currentMember, type);
    }

    public async Task<ServiceResult> SendTestMemberEmail(MemberServiceRequest request, EmailType type)
    {
        var currentMemberId = request.CurrentMemberId;

        var currentMember = await GetSiteAdminRestrictedContent(currentMemberId,
            x => x.MemberRepository.GetById(currentMemberId));

        return await _memberEmailService.SendTestEmail(request, null, currentMember, type);
    }

    public async Task<ServiceResult> UpdateChapterEmail(MemberChapterServiceRequest request, EmailType type,
        UpdateEmail model)
    {
        var (chapterEmail, siteEmail) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterEmailRepository.GetByChapterId(request.ChapterId, type),
            x => x.EmailRepository.GetByType(type));

        if (!siteEmail.Overridable)
        {
            return ServiceResult.Failure("This email cannot be customised");
        }

        chapterEmail ??= new ChapterEmail
        {
            ChapterId = request.ChapterId,
            Type = type
        };

        chapterEmail.HtmlContent = model.HtmlContent;
        chapterEmail.Subject = model.Subject;

        var validationResult = ValidateChapterEmail(chapterEmail);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        _unitOfWork.ChapterEmailRepository.Upsert(chapterEmail);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UpdateEmail(Guid currentMemberId, EmailType type, UpdateEmail model)
    {
        var existing = await GetSiteAdminRestrictedContent(currentMemberId,
            x => x.EmailRepository.GetByType(type));

        existing.HtmlContent = model.HtmlContent;
        existing.Overridable = model.Overridable;
        existing.Subject = model.Subject;

        var validationResult = ValidateEmail(existing);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        _unitOfWork.EmailRepository.Update(existing);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    private static ServiceResult ValidateChapterEmail(ChapterEmail chapterEmail)
    {
        if (!Enum.IsDefined(typeof(EmailType), chapterEmail.Type) || chapterEmail.Type == EmailType.None)
        {
            return ServiceResult.Failure("Invalid type");
        }

        if (string.IsNullOrWhiteSpace(chapterEmail.HtmlContent) ||
            string.IsNullOrWhiteSpace(chapterEmail.Subject))
        {
            return ServiceResult.Failure("Some required fields are missing");
        }

        return ServiceResult.Successful();
    }

    private static ServiceResult ValidateEmail(Email email)
    {
        if (!Enum.IsDefined(typeof(EmailType), email.Type) || email.Type == EmailType.None)
        {
            return ServiceResult.Failure("Invalid type");
        }

        if (string.IsNullOrWhiteSpace(email.HtmlContent) ||
            string.IsNullOrWhiteSpace(email.Subject))
        {
            return ServiceResult.Failure("Some required fields are missing");
        }

        return ServiceResult.Successful();
    }
}
