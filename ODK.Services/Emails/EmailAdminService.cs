using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Features;
using ODK.Core.Subscriptions;
using ODK.Data.Core;
using ODK.Data.Core.Deferred;
using ODK.Services.Authorization;
using ODK.Services.Emails.Models;
using ODK.Services.Emails.Parameters;
using ODK.Services.Emails.Validation;
using ODK.Services.Emails.ViewModels;
using ODK.Services.Html;
using ODK.Services.Members;

namespace ODK.Services.Emails;

public class EmailAdminService : OdkAdminServiceBase, IEmailAdminService
{
    private const string NotPermitted = "Not permitted";

    /* Templates are hand-written HTML rather than editor output, so markup left open or closed out of
       order is a typo worth reporting rather than something to quietly recover from. */
    private static readonly HtmlValidatorOptions TemplateHtmlOptions = new()
    {
        AllowLinks = true,
        RequireWellFormed = true
    };

    private readonly IAuthorizationService _authorizationService;
    private readonly IHtmlValidator _htmlValidator;
    private readonly IMemberEmailService _memberEmailService;
    private readonly IUnitOfWork _unitOfWork;

    public EmailAdminService(
        IUnitOfWork unitOfWork,
        IMemberEmailService memberEmailService,
        IAuthorizationService authorizationService,
        IHtmlValidator htmlValidator)
        : base(unitOfWork)
    {
        _authorizationService = authorizationService;
        _htmlValidator = htmlValidator;
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

        var (chapterEmail, siteEmail, settings, siteSettings, ownerSubscriptionFeatures) =
            await GetChapterAdminRestrictedContent(
                request,
                x => x.ChapterEmailRepository.GetByChapterId(chapter.Id, type),
                x => x.EmailRepository.GetByType(type),
                x => x.ChapterEmailSettingsRepository.GetByChapterIdOrDefault(chapter.Id),
                x => x.SiteEmailSettingsRepository.Get(request.Platform),
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
            },
            RecipientType = siteEmail.RecipientType,
            Title = EmailTitle.For(siteSettings, settings, siteEmail.RecipientType)
        };
    }

    public async Task<ChapterEmailsAdminPageViewModel> GetChapterEmails(
        IMemberChapterAdminServiceRequest request)
    {
        var chapter = request.Chapter;

        var (chapterEmails, siteEmails, settings, siteSettings, ownerSubscriptionFeatures) =
            await GetChapterAdminRestrictedContent(
                request,
                x => x.ChapterEmailRepository.GetByChapterId(chapter.Id),
                x => x.EmailRepository.GetAll(),
                x => x.ChapterEmailSettingsRepository.GetByChapterIdOrDefault(chapter.Id),
                x => x.SiteEmailSettingsRepository.Get(request.Platform),
                OwnerSubscriptionFeatures(chapter.Id));

        var chapterEmailDictionary = chapterEmails.ToDictionary(x => x.Type);

        var emails = new List<ChapterEmailListItemViewModel>();

        foreach (var siteEmail in siteEmails.OrderBy(x => x.Type))
        {
            if (!siteEmail.Overridable)
            {
                continue;
            }

            if (!chapterEmailDictionary.TryGetValue(siteEmail.Type, out var chapterEmail))
            {
                chapterEmail = new ChapterEmail
                {
                    ChapterId = chapter.Id,
                    HtmlContent = siteEmail.HtmlContent,
                    Subject = siteEmail.Subject,
                    Type = siteEmail.Type
                };
            }

            emails.Add(new ChapterEmailListItemViewModel
            {
                Email = chapterEmail,
                RecipientType = siteEmail.RecipientType
            });
        }

        return new ChapterEmailsAdminPageViewModel
        {
            CanEdit = CanEditEmails(ownerSubscriptionFeatures),
            Emails = emails,
            Settings = settings,
            SiteAdminTitle = siteSettings.AdminTitle,
            SiteMemberTitle = siteSettings.MemberTitle
        };
    }

    public async Task<EmailAdminPageViewModel> GetEmail(IMemberServiceRequest request, EmailType type)
    {
        var (email, siteSettings) = await GetSiteAdminRestrictedContent(
            request,
            x => x.EmailRepository.GetByType(type),
            x => x.SiteEmailSettingsRepository.Get(request.Platform));

        return new EmailAdminPageViewModel
        {
            Email = email,
            // No chapter settings: this is the site's own copy of the template.
            Title = EmailTitle.For(siteSettings, chapterEmailSettings: null, email.RecipientType)
        };
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

    public async Task<ServiceResult> UpdateChapterEmailSettings(
        IMemberChapterAdminServiceRequest request,
        ChapterEmailSettingsUpdateModel model)
    {
        var chapter = request.Chapter;

        var (settings, ownerSubscriptionFeatures) = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterEmailSettingsRepository.GetByChapterIdOrDefault(chapter.Id),
            OwnerSubscriptionFeatures(chapter.Id));

        // The form renders read-only without the feature, but that is presentation - this is what
        // withholds it. Anything already set is left alone and keeps being used.
        if (!CanEditEmails(ownerSubscriptionFeatures))
        {
            return ServiceResult.Failure(NotPermitted);
        }

        settings ??= new ChapterEmailSettings
        {
            ChapterId = chapter.Id
        };

        /* Blank is stored as null rather than as an empty string, so the row says the group has not set a
           title rather than that it set one to nothing. Both read the same to the send path, but only null
           reads that way to someone looking at the data. */
        settings.AdminTitle = Unset(model.AdminTitle);
        settings.MemberTitle = Unset(model.MemberTitle);

        _unitOfWork.ChapterEmailSettingsRepository.Upsert(settings);
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

    /* Authorised but deliberately not gated on the custom emails feature or on Overridable, unlike
       UpdateChapterEmail. Those refusals are about whether a template may be saved at all, and the form
       is read-only when either applies, so no request gets this far; answering them here would put
       "This email cannot be customised" under a field as if it were a markup error. */
    public async Task<ServiceResult> ValidateChapterEmailHtml(
        IMemberChapterAdminServiceRequest request,
        EmailType type,
        string? htmlContent)
    {
        await AssertMemberIsChapterAdmin(request);

        return ValidateHtml(type, htmlContent);
    }

    public ServiceResult ValidateEmailHtml(IMemberServiceRequest request, EmailType type, string? htmlContent)
    {
        AssertMemberIsSiteAdmin(request.CurrentMember);

        return ValidateHtml(type, htmlContent);
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

    private static string? Unset(string? value) => !string.IsNullOrWhiteSpace(value) ? value : null;

    /* Checked against everything the type supplies, not the narrower list a group is offered: a group
       template using platform.baseurl still resolves, so rejecting it would fail a working email. */
    private static ServiceResult ValidatePlaceholders(EmailType type, string subject, string htmlContent)
    {
        var supplied = EmailTemplateParameters.ForSite(type);

        var unknown = EmailTemplateValidator.UnknownPlaceholders(subject, supplied)
            .Concat(EmailTemplateValidator.UnknownPlaceholders(htmlContent, supplied))
            .Distinct(EmailParameterComparer.Default)
            .ToArray();

        return unknown.Length > 0
            ? ServiceResult.Failure(
                $"Unknown placeholder{(unknown.Length > 1 ? "s" : "")}: " +
                string.Join(", ", unknown.Select(x => $"{{{x}}}")))
            : ServiceResult.Successful();
    }

    private bool CanEditEmails(IReadOnlyCollection<SiteSubscriptionFeature> ownerSubscriptionFeatures)
        => _authorizationService.ChapterHasAccess(ownerSubscriptionFeatures, SiteFeatureType.CustomEmails);

    private ServiceResult ValidateChapterEmail(ChapterEmail chapterEmail)
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

        var placeholderResult = ValidatePlaceholders(
            chapterEmail.Type, chapterEmail.Subject, chapterEmail.HtmlContent);

        return !placeholderResult.Success
            ? placeholderResult
            : ValidateHtml(chapterEmail.Type, chapterEmail.HtmlContent);
    }

    private ServiceResult ValidateEmail(Email email)
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

        var placeholderResult = ValidatePlaceholders(email.Type, email.Subject, email.HtmlContent);

        return !placeholderResult.Success
            ? placeholderResult
            : ValidateHtml(email.Type, email.HtmlContent);
    }


    /* The layout is exempt. It is the full HTML document every other email is rendered into -
       <html>, <head>, a stylesheet - so the allow-list tuned for rich text would reject it outright.
       Only the site admin edits it, and it is being made non-overridable. Subjects are not checked
       either: they are plain text, so a stray angle bracket is not markup. */
    private ServiceResult ValidateHtml(EmailType type, string? htmlContent) => type == EmailType.Layout
        ? ServiceResult.Successful()
        : _htmlValidator.Validate(htmlContent, TemplateHtmlOptions);
}