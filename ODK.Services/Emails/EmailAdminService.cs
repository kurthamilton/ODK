using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Data.Core;

namespace ODK.Services.Emails;

public class EmailAdminService : OdkAdminServiceBase, IEmailAdminService
{
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;

    public EmailAdminService(IUnitOfWork unitOfWork, IEmailService emailService)
        : base(unitOfWork)
    {
        _emailService = emailService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> DeleteChapterEmail(AdminServiceRequest request, EmailType type)
    {
        var chapterEmail = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterEmailRepository.GetByChapterId(request.ChapterId, type));

        OdkAssertions.Exists(chapterEmail);

        _unitOfWork.ChapterEmailRepository.Delete(chapterEmail);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ChapterEmail> GetChapterEmail(AdminServiceRequest request, EmailType type)
    {
        var (chapterEmail, email) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterEmailRepository.GetByChapterId(request.ChapterId, type),
            x => x.EmailRepository.GetByType(type));

        if (chapterEmail != null)
        {
            return chapterEmail;
        }

        return new ChapterEmail
        {
            ChapterId = request.ChapterId,
            HtmlContent = email.HtmlContent,
            Subject = email.Subject,
            Type = email.Type
        };
    }

    public async Task<IReadOnlyCollection<ChapterEmail>> GetChapterEmails(AdminServiceRequest request)
    {
        var (chapterEmails, emails) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterEmailRepository.GetByChapterId(request.ChapterId),
            x => x.EmailRepository.GetAll());

        var chapterEmailDictionary = chapterEmails.ToDictionary(x => x.Type);
        var emailDictionary = emails.ToDictionary(x => x.Type);

        var defaultEmails = new List<ChapterEmail>();

        foreach (EmailType type in Enum.GetValues(typeof(EmailType)))
        {
            if (type == EmailType.None)
            {
                continue;
            }

            if (!chapterEmailDictionary.ContainsKey(type))
            {
                if (!emailDictionary.TryGetValue(type, out var email))
                {
                    continue;
                }

                defaultEmails.Add(new ChapterEmail
                {
                    ChapterId = request.ChapterId,
                    HtmlContent = email.HtmlContent,
                    Subject = email.Subject,
                    Type = type
                });
            }
        }

        return chapterEmails
            .Union(defaultEmails)
            .OrderBy(x => x.Type)
            .ToArray();
    }

    public async Task<ChapterEmailSettings?> GetChapterEmailSettings(AdminServiceRequest request)
    {
        return await GetChapterAdminRestrictedContent(request,
            x => x.ChapterEmailSettingsRepository.GetByChapterId(request.ChapterId));
    }

    public async Task<Email> GetEmail(Guid currentMemberId, EmailType type)
    {
        return await GetSuperAdminRestrictedContent(currentMemberId,
            x => x.EmailRepository.GetByType(type));
    }

    public async Task<IReadOnlyCollection<Email>> GetEmails(Guid currentMemberId)
    {
        return await GetSuperAdminRestrictedContent(currentMemberId,
            x => x.EmailRepository.GetAll());
    }

    public async Task<ServiceResult> SendTestEmail(AdminServiceRequest request, EmailType type)
    {
        var (chapterId, currentMemberId) = (request.ChapterId, request.CurrentMemberId);

        var (chapter, currentMember) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(chapterId),
            x => x.MemberRepository.GetById(currentMemberId));

        return await _emailService.SendEmail(chapter, currentMember.GetEmailAddressee(), type, 
            new Dictionary<string, string>
            {
                { "chapter.name", chapter.Name },
                { "member.emailAddress", currentMember.FirstName },
                { "member.firstName", currentMember.FirstName },
                { "member.lastName", currentMember.FirstName }
            });
    }

    public async Task<ServiceResult> SendTestEmail(Guid currentMemberId, EmailType type)
    {        
        var currentMember = await GetSuperAdminRestrictedContent(currentMemberId,
            x => x.MemberRepository.GetById(currentMemberId));

        return await _emailService.SendEmail(null, currentMember.GetEmailAddressee(), type,
            new Dictionary<string, string>
            {
                { "member.emailAddress", currentMember.FirstName },
                { "member.firstName", currentMember.FirstName },
                { "member.lastName", currentMember.FirstName }
            });
    }

    public async Task<ServiceResult> UpdateChapterEmail(AdminServiceRequest request, EmailType type, 
        UpdateEmail update)
    {
        var chapterEmail = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterEmailRepository.GetByChapterId(request.ChapterId, type));

        if (chapterEmail == null)
        {
            chapterEmail = new ChapterEmail
            {
                ChapterId = request.ChapterId,                
                Type = type
            };            
        }

        chapterEmail.HtmlContent = update.HtmlContent;
        chapterEmail.Subject = update.Subject;

        var validationResult = ValidateChapterEmail(chapterEmail);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        _unitOfWork.ChapterEmailRepository.Upsert(chapterEmail);        
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UpdateChapterEmailSettings(AdminServiceRequest request, UpdateChapterEmailSettings model)
    {
        var settings = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterEmailSettingsRepository.GetByChapterId(request.ChapterId));

        if (settings == null)
        {
            settings = new ChapterEmailSettings();
        }

        settings.FromEmailAddress = model.FromAddress;
        settings.FromName = model.FromName;

        if (settings.ChapterId == Guid.Empty)
        {
            settings.ChapterId = request.ChapterId;
            _unitOfWork.ChapterEmailSettingsRepository.Add(settings);
        }
        else
        {
            _unitOfWork.ChapterEmailSettingsRepository.Update(settings);
        }

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UpdateEmail(Guid currentMemberId, EmailType type, UpdateEmail email)
    {
        var existing = await GetSuperAdminRestrictedContent(currentMemberId,
            x => x.EmailRepository.GetByType(type));

        existing.HtmlContent = email.HtmlContent;
        existing.Subject = email.Subject;

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
