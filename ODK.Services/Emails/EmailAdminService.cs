using ODK.Core;
using ODK.Core.Emails;
using ODK.Core.Features;
using ODK.Core.Subscriptions;
using ODK.Data.Core;
using ODK.Data.Core.Deferred;
using ODK.Services.Authorization;
using ODK.Services.Emails.Models;
using ODK.Services.Emails.ViewModels;
using ODK.Services.Members;

namespace ODK.Services.Emails;

public class EmailAdminService : OdkAdminServiceBase, IEmailAdminService
{
    private const string NotPermitted = "Not permitted";

    private readonly IAuthorizationService _authorizationService;
    private readonly IMemberEmailService _memberEmailService;
    private readonly IUnitOfWork _unitOfWork;

    public EmailAdminService(
        IUnitOfWork unitOfWork,
        IMemberEmailService memberEmailService,
        IAuthorizationService authorizationService)
        : base(unitOfWork)
    {
        _authorizationService = authorizationService;
        _memberEmailService = memberEmailService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> DeleteChapterEmail(IMemberChapterAdminServiceRequest request, EmailType type)
    {
        var chapter = request.Chapter;

        /* Deliberately not gated on the feature. Deleting the override restores the standard email,
           which is the state a group without custom emails would be in anyway - blocking it would
           strand a group with wording it can neither change nor remove. */
        var chapterEmail = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterEmailRepository.GetByChapterId(chapter.Id, type));

        OdkAssertions.Exists(chapterEmail);

        _unitOfWork.ChapterEmailRepository.Delete(chapterEmail);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ChapterEmailAdminPageViewModel> GetChapterEmail(
        IMemberChapterAdminServiceRequest request, EmailType type)
    {
        var chapter = request.Chapter;

        var (chapterEmail, siteEmail, ownerSubscriptionFeatures) = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterEmailRepository.GetByChapterId(chapter.Id, type),
            x => x.EmailRepository.GetByType(type),
            OwnerSubscriptionFeatures(chapter.Id));

        return new ChapterEmailAdminPageViewModel
        {
            CanEdit = CanEditEmails(ownerSubscriptionFeatures),
            Email = chapterEmail ?? new ChapterEmail
            {
                ChapterId = chapter.Id,
                HtmlContent = siteEmail.HtmlContent,
                Subject = siteEmail.Subject,
                Type = siteEmail.Type
            }
        };
    }

    public async Task<IReadOnlyCollection<ChapterEmail>> GetChapterEmails(
        IMemberChapterAdminServiceRequest request)
    {
        var chapter = request.Chapter;

        var (chapterEmails, siteEmails) = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterEmailRepository.GetByChapterId(chapter.Id),
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
                    ChapterId = chapter.Id,
                    HtmlContent = siteEmail.HtmlContent,
                    Subject = siteEmail.Subject,
                    Type = siteEmail.Type
                });
            }
        }

        return emails;
    }

    public async Task<Email> GetEmail(IMemberServiceRequest request, EmailType type)
    {
        return await GetSiteAdminRestrictedContent(request,
            x => x.EmailRepository.GetByType(type));
    }

    public async Task<IReadOnlyCollection<Email>> GetEmails(IMemberServiceRequest request)
    {
        return await GetSiteAdminRestrictedContent(request,
            x => x.EmailRepository.GetAll());
    }

    public async Task<ServiceResult> SendTestEmail(IMemberChapterAdminServiceRequest request, EmailType type)
    {
        var (chapter, currentMember) = (request.Chapter, request.CurrentMember);

        await AssertMemberIsChapterAdmin(request);

        return await _memberEmailService.SendTestEmail(request, chapter, currentMember, type);
    }

    public async Task<ServiceResult> SendTestMemberEmail(IMemberServiceRequest request, EmailType type)
    {
        AssertMemberIsSiteAdmin(request.CurrentMember);

        return await _memberEmailService.SendTestEmail(request, null, request.CurrentMember, type);
    }

    public async Task<ServiceResult> UpdateChapterEmail(
        IMemberChapterAdminServiceRequest request,
        EmailType type,
        EmailUpdateModel model)
    {
        var chapter = request.Chapter;

        var (chapterEmail, siteEmail, ownerSubscriptionFeatures) = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterEmailRepository.GetByChapterId(chapter.Id, type),
            x => x.EmailRepository.GetByType(type),
            OwnerSubscriptionFeatures(chapter.Id));

        if (!siteEmail.Overridable)
        {
            return ServiceResult.Failure("This email cannot be customised");
        }

        // The form renders read-only without the feature, but that is presentation - this is what
        // withholds it. An existing override is left alone and keeps sending; only changing it is blocked.
        if (!CanEditEmails(ownerSubscriptionFeatures))
        {
            return ServiceResult.Failure(NotPermitted);
        }

        chapterEmail ??= new ChapterEmail
        {
            ChapterId = chapter.Id,
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

    public async Task<ServiceResult> UpdateEmail(IMemberServiceRequest request, EmailType type, EmailUpdateModel model)
    {
        var existing = await GetSiteAdminRestrictedContent(request,
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

    /* Batched into whichever query the caller is already running rather than fetched on its own, so
       reading the feature costs no extra round trip. */
    private static Func<IUnitOfWork, IDeferredQueryMultiple<SiteSubscriptionFeature>> OwnerSubscriptionFeatures(
        Guid chapterId)
        => x => x.MemberSiteSubscriptionRecordRepository
            .Query(x => x.Current().ForChapterOwner(chapterId).Active())
            .SiteSubscription()
            .Features()
            .GetAll();

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

    private bool CanEditEmails(IReadOnlyCollection<SiteSubscriptionFeature> ownerSubscriptionFeatures)
        => _authorizationService.ChapterHasAccess(ownerSubscriptionFeatures, SiteFeatureType.CustomEmails);
}