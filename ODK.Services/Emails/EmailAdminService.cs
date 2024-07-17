using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Exceptions;
using ODK.Data.Core;

namespace ODK.Services.Emails;

public class EmailAdminService : OdkAdminServiceBase, IEmailAdminService
{
    private readonly IUnitOfWork _unitOfWork;

    public EmailAdminService(IUnitOfWork unitOfWork)
        : base(unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> AddChapterEmailProvider(Guid currentMemberId, Guid chapterId, 
        UpdateChapterEmailProvider model)
    {
        var (chapterAdminMembers, existing) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapterId),
            x => x.ChapterEmailProviderRepository.GetByChapterId(chapterId));

        AssertMemberIsChapterAdmin(currentMemberId, chapterId, chapterAdminMembers);

        var provider = new ChapterEmailProvider
        {
            BatchSize = model.BatchSize,
            ChapterId = chapterId,
            DailyLimit = model.DailyLimit,
            FromEmailAddress = model.FromEmailAddress,
            FromName = model.FromName,
            Order = existing.Count + 1,
            SmtpLogin = model.SmtpLogin,
            SmtpPassword = model.SmtpPassword,
            SmtpPort = model.SmtpPort,
            SmtpServer = model.SmtpServer
        };

        var validationResult = ValidateChapterEmailProvider(provider);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        _unitOfWork.ChapterEmailProviderRepository.Add(provider);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> DeleteChapterEmail(Guid currentMemberId, Guid chapterId, EmailType type)
    {
        var (chapterAdminMembers, chapterEmail) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapterId),
            x => x.ChapterEmailRepository.GetByChapterId(chapterId, type));

        if (chapterEmail == null)
        {
            throw new OdkNotFoundException();
        }

        AssertMemberIsChapterAdmin(currentMemberId, chapterId, chapterAdminMembers);

        _unitOfWork.ChapterEmailRepository.Delete(chapterEmail);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> DeleteChapterEmail(Guid currentMemberId, string chapterName, EmailType type)
    {
        var chapter = await _unitOfWork.ChapterRepository.GetByName(chapterName).RunAsync();
        if (chapter == null)
        {
            return ServiceResult.Failure("Chapter not found");
        }

        return await DeleteChapterEmail(currentMemberId, chapter.Id, type);
    }

    public async Task DeleteChapterEmailProvider(Guid currentMemberId, Guid chapterEmailProviderId)
    {
        var provider = await GetChapterEmailProvider(currentMemberId, chapterEmailProviderId);
        if (provider == null)
        {
            return;
        }

        _unitOfWork.ChapterEmailProviderRepository.Delete(provider);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<ChapterEmail> GetChapterEmail(Guid currentMemberId, Guid chapterId, EmailType type)
    {
        var (chapterAdminMembers, chapterEmail, email) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapterId),
            x => x.ChapterEmailRepository.GetByChapterId(chapterId, type),
            x => x.EmailRepository.GetByType(type));

        AssertMemberIsChapterAdmin(currentMemberId, chapterId, chapterAdminMembers);

        if (chapterEmail != null)
        {
            return chapterEmail;
        }

        return new ChapterEmail
        {
            ChapterId = chapterId,
            HtmlContent = email.HtmlContent,
            Subject = email.Subject,
            Type = email.Type
        };
    }

    public async Task<ChapterEmailProvider?> GetChapterEmailProvider(Guid currentMemberId, Guid chapterEmailProviderId)
    {        
        var provider = await _unitOfWork.ChapterEmailProviderRepository.GetByIdOrDefault(chapterEmailProviderId).RunAsync();
        if (provider == null)
        {
            return null;
        }

        await AssertMemberIsChapterAdmin(currentMemberId, provider.ChapterId);

        return provider;
    }

    public async Task<IReadOnlyCollection<ChapterEmailProvider>> GetChapterEmailProviders(Guid currentMemberId, Guid chapterId)
    {
        var (chapterAdminMembers, providers) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapterId),
            x => x.ChapterEmailProviderRepository.GetByChapterId(chapterId));

        AssertMemberIsChapterAdmin(currentMemberId, chapterId, chapterAdminMembers);

        return providers;
    }

    public async Task<IReadOnlyCollection<ChapterEmail>> GetChapterEmails(Guid currentMemberId, Guid chapterId)
    {
        var (chapterAdminMembers, chapterEmails, emails) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapterId),
            x => x.ChapterEmailRepository.GetByChapterId(chapterId),
            x => x.EmailRepository.GetAll());

        AssertMemberIsChapterAdmin(currentMemberId, chapterId, chapterAdminMembers);
        
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
                    ChapterId = chapterId,
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

    public async Task<Email> GetEmail(Guid currentMemberId, Guid currentChapterId, EmailType type)
    {
        var (chapterAdminMembers, email, currentMember) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(currentChapterId),
            x => x.EmailRepository.GetByType(type),
            x => x.MemberRepository.GetById(currentMemberId));

        AssertMemberIsChapterSuperAdmin(currentMember, currentChapterId, chapterAdminMembers);

        return email;
    }

    public async Task<IReadOnlyCollection<Email>> GetEmails(Guid currentMemberId, Guid currentChapterId)
    {
        var (chapterAdminMembers, emails, currentMember) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(currentChapterId),
            x => x.EmailRepository.GetAll(),
            x => x.MemberRepository.GetById(currentMemberId));

        AssertMemberIsChapterSuperAdmin(currentMember, currentChapterId, chapterAdminMembers);

        return emails;
    }

    public async Task<ServiceResult> UpdateChapterEmail(Guid currentMemberId, Guid chapterId, EmailType type, 
        UpdateEmail update)
    {
        var (chapterAdminMembers, chapterEmail) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapterId),
            x => x.ChapterEmailRepository.GetByChapterId(chapterId, type));

        AssertMemberIsChapterAdmin(currentMemberId, chapterId, chapterAdminMembers);

        if (chapterEmail == null)
        {
            chapterEmail = new ChapterEmail
            {
                ChapterId = chapterId,                
                Type = type
            };            
        }

        chapterEmail.HtmlContent = update.HtmlContent;
        chapterEmail.Subject = update.Subject;

        ServiceResult validationResult = ValidateChapterEmail(chapterEmail);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        _unitOfWork.ChapterEmailRepository.Upsert(chapterEmail);        
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UpdateChapterEmailProvider(Guid currentMemberId, Guid chapterEmailProviderId,
        UpdateChapterEmailProvider update)
    {
        var provider = await GetChapterEmailProvider(currentMemberId, chapterEmailProviderId);
        if (provider == null)
        {
            return ServiceResult.Failure("Chapter email provider not found");
        }

        provider.BatchSize = update.BatchSize;
        provider.DailyLimit = update.DailyLimit;
        provider.FromEmailAddress = update.FromEmailAddress;
        provider.FromName = update.FromName;
        provider.SmtpLogin = update.SmtpLogin;
        provider.SmtpPassword = update.SmtpPassword;
        provider.SmtpPort = update.SmtpPort;
        provider.SmtpServer = update.SmtpServer;

        var validationResult = ValidateChapterEmailProvider(provider);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        _unitOfWork.ChapterEmailProviderRepository.Update(provider);
        await _unitOfWork.SaveChangesAsync();
        
        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UpdateEmail(Guid currentMemberId, Guid currentChapterId, EmailType type, UpdateEmail email)
    {
        var (chapterAdminMembers, existing, currentMember) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(currentChapterId),
            x => x.EmailRepository.GetByType(type),
            x => x.MemberRepository.GetById(currentMemberId));

        AssertMemberIsChapterSuperAdmin(currentMember, currentChapterId, chapterAdminMembers);
        
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
    
    private static ServiceResult ValidateChapterEmailProvider(ChapterEmailProvider provider)
    {
        if (string.IsNullOrWhiteSpace(provider.FromEmailAddress) ||
            string.IsNullOrWhiteSpace(provider.FromName) ||
            string.IsNullOrWhiteSpace(provider.SmtpLogin) ||
            string.IsNullOrWhiteSpace(provider.SmtpPassword) ||
            provider.SmtpPort == 0 ||
            provider.DailyLimit <= 0 ||
            provider.BatchSize <= 0)
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
