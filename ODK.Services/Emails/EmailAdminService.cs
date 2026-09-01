using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Extensions;
using ODK.Core.Features;
using ODK.Core.Subscriptions;
using ODK.Core.Utils;
using ODK.Data.Core;
using ODK.Data.Core.Deferred;
using ODK.Services.Authorization;
using ODK.Services.Emails.Models;
using ODK.Services.Emails.Parameters;
using ODK.Services.Emails.Validation;
using ODK.Services.Emails.ViewModels;
using ODK.Services.Html;
using ODK.Services.Members;
using ODK.Services.Web;

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
    private readonly ISiteEmailSettingsProvider _siteEmailSettingsProvider;
    private readonly SiteSubscriptionCooldown _siteSubscriptionCooldown;
    private readonly ITestEmailParametersFactory _testEmailParametersFactory;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUrlProviderFactory _urlProviderFactory;

    public EmailAdminService(
        IUnitOfWork unitOfWork,
        IMemberEmailService memberEmailService,
        IAuthorizationService authorizationService,
        IHtmlValidator htmlValidator,
        IUrlProviderFactory urlProviderFactory,
        ISiteEmailSettingsProvider siteEmailSettingsProvider,
        SiteSubscriptionCooldown siteSubscriptionCooldown,
        ITestEmailParametersFactory testEmailParametersFactory)
        : base(unitOfWork)
    {
        _authorizationService = authorizationService;
        _htmlValidator = htmlValidator;
        _memberEmailService = memberEmailService;
        _siteEmailSettingsProvider = siteEmailSettingsProvider;
        _siteSubscriptionCooldown = siteSubscriptionCooldown;
        _testEmailParametersFactory = testEmailParametersFactory;
        _unitOfWork = unitOfWork;
        _urlProviderFactory = urlProviderFactory;
    }

    public async Task<ChapterEmailAdminPageViewModel> GetChapterEmail(
        IMemberChapterAdminServiceRequest request, EmailType type)
    {
        var (chapter, currentMember) = (request.Chapter, request.CurrentMember);

        var (chapterEmail, siteEmail, settings, ownerSubscriptionFeatures, preferences) =
            await GetChapterAdminRestrictedContent(
                request,
                x => x.ChapterEmailRepository.GetByChapterId(chapter.Id, type),
                x => x.EmailRepository.GetByType(type),
                x => x.ChapterEmailSettingsRepository.GetByChapterIdOrDefault(chapter.Id),
                OwnerSubscriptionFeatures(chapter.Id, _siteSubscriptionCooldown),
                x => x.MemberPreferencesRepository.GetByMemberIdOrDefault(currentMember.Id));

        // The group's own platform, not the request's - the same email is addressed the same way whichever
        // site an admin edits it from.
        var siteSettings = _siteEmailSettingsProvider.Get(chapter.Platform);

        var title = EmailTitle.For(siteSettings, settings, siteEmail.RecipientType);

        /* Built from the same type the send path fills in, so the values shown are the ones an email would
           actually carry and cannot drift from them. The group is fixed on this page, so everything about it
           already has a value; only what the email is about is still unknown. */
        var urlProvider = await _urlProviderFactory.Create(request);

        var coreParameters = new EmailParameters
        {
            GroupName = chapter.FullName,
            GroupUrl = urlProvider.GroupUrl(chapter),
            PlatformUrl = urlProvider.BaseUrl()
        };

        var customParameters = await _testEmailParametersFactory.Create(
            request,
            type,
            currentMember,
            LocaleUtils.GetCultureOrDefault(preferences?.Locale),
            chapter);

        var resolved = new Dictionary<string, string>()
            .Merge(coreParameters.ToDictionary())
            .Merge(customParameters.ToDictionary())
            .AsReadOnly();

        return new ChapterEmailAdminPageViewModel
        {
            CanOverride = CanOverrideEmails(ownerSubscriptionFeatures),
            /* Carries the type and the group where there is no override, so the page can render a form for
               an email the group has yet to customise. Its wording stays unset: a field the group has not
               overridden is shown as inherited rather than as a copy it has made. */
            Email = chapterEmail ?? new ChapterEmail
            {
                ChapterId = chapter.Id,
                Type = siteEmail.Type
            },
            Parameters = Parameters(EmailTemplateParameters.ForGroup(siteEmail.Type), title, resolved),
            RecipientType = siteEmail.RecipientType,
            SiteEmail = siteEmail,
            Title = title
        };
    }

    public async Task<ChapterEmailsAdminPageViewModel> GetChapterEmails(
        IMemberChapterAdminServiceRequest request)
    {
        var chapter = request.Chapter;

        var (chapterEmails, siteEmails, settings, ownerSubscriptionFeatures) =
            await GetChapterAdminRestrictedContent(
                request,
                x => x.ChapterEmailRepository.GetByChapterId(chapter.Id),
                x => x.EmailRepository.GetAll(),
                x => x.ChapterEmailSettingsRepository.GetByChapterIdOrDefault(chapter.Id),
                OwnerSubscriptionFeatures(chapter.Id, _siteSubscriptionCooldown));

        var siteSettings = _siteEmailSettingsProvider.Get(chapter.Platform);

        var chapterEmailDictionary = chapterEmails.ToDictionary(x => x.Type);

        var emails = new List<ChapterEmailListItemViewModel>();

        foreach (var siteEmail in siteEmails.OrderBy(x => x.Type))
        {
            if (!siteEmail.IsGroupEmail)
            {
                continue;
            }

            /* Stands in for a group that has not customised this template, so its wording stays unset - the
               list reports which fields the group overrides, and filling these with the site's would report
               every template as fully customised. Only the type is needed: the name comes from it. */
            if (!chapterEmailDictionary.TryGetValue(siteEmail.Type, out var chapterEmail))
            {
                chapterEmail = new ChapterEmail
                {
                    ChapterId = chapter.Id,
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
            CanEdit = CanOverrideEmails(ownerSubscriptionFeatures),
            Emails = emails,
            Settings = settings,
            SiteAdminTitle = siteSettings.AdminTitle,
            SiteMemberTitle = siteSettings.MemberTitle
        };
    }

    public async Task<EmailAdminPageViewModel> GetSiteEmail(IMemberServiceRequest request, EmailType type)
    {
        var email = await GetSiteAdminRestrictedContent(
            request,
            x => x.EmailRepository.GetByType(type));

        var siteSettings = _siteEmailSettingsProvider.Get(request.Platform);

        // No chapter settings: this is the site's own copy of the template.
        var title = EmailTitle.For(siteSettings, chapterEmailSettings: null, email.RecipientType);

        /* The platform is the same whatever the email is about, so its URL has a value here. The group
           parameters deliberately do not: this is the template every group starts from, so showing one
           group's name would be showing a value the template does not have. */
        var urlProvider = await _urlProviderFactory.Create(request);
        var resolved = new EmailParameters
        {
            PlatformUrl = urlProvider.BaseUrl()
        }.ToDictionary();

        return new EmailAdminPageViewModel
        {
            Email = email,
            Parameters = Parameters(EmailTemplateParameters.ForSite(email.Type), title, resolved),
            Title = title
        };
    }

    public async Task<IReadOnlyCollection<Email>> GetSiteEmails(IMemberServiceRequest request)
    {
        return await GetSiteAdminRestrictedContent(request,
            x => x.EmailRepository.GetAll());
    }

    public async Task<RenderedEmail> PreviewChapterEmail(
        IMemberChapterAdminServiceRequest request,
        EmailType type,
        ChapterEmailUpdateModel model)
    {
        var (chapter, currentMember) = (request.Chapter, request.CurrentMember);

        var (chapterEmail, siteEmail) = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterEmailRepository.GetByChapterId(chapter.Id, type),
            x => x.EmailRepository.GetByType(type));

        /* Resolved exactly as UpdateChapterEmail resolves it, so the preview shows what saving this form
           would send rather than what is stored.

           The fall through to the site's wording is stated here rather than left to the renderer's own
           fallback: the stored row may still hold an override this form is turning off, which is what that
           fallback would find and show. */
        var bodyHtml = Resolve(model.OverrideBody, model.BodyHtml, chapterEmail?.BodyHtml)
            ?? siteEmail.BodyHtml;
        var subject = Resolve(model.OverrideSubject, model.Subject, chapterEmail?.Subject)
            ?? siteEmail.Subject;

        return await _memberEmailService.RenderTestEmail(
            request, chapter, currentMember, type, subject, bodyHtml);
    }

    public async Task<RenderedEmail> PreviewSiteEmail(
        IMemberServiceRequest request,
        EmailType type,
        string subject,
        string body)
    {
        AssertMemberIsSiteAdmin(request.CurrentMember);

        // Nothing to resolve: the site's form posts both fields, so what it holds is what it sends.
        return await _memberEmailService.RenderTestEmail(
            request, null, request.CurrentMember, type, subject, body);
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
        ChapterEmailUpdateModel model)
    {
        var chapter = request.Chapter;

        var (chapterEmail, siteEmail, ownerSubscriptionFeatures) = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterEmailRepository.GetByChapterId(chapter.Id, type),
            x => x.EmailRepository.GetByType(type),
            OwnerSubscriptionFeatures(chapter.Id, _siteSubscriptionCooldown));

        if (!siteEmail.IsGroupEmail)
        {
            return ServiceResult.Failure("This email cannot be customised");
        }

        /* Blank is stored as null rather than as an empty string, so the row says the group has not
           overridden the field rather than that it overrode it with nothing. Each is independent: setting
           one leaves the other inheriting the site's. */
        var bodyHtml = Resolve(
            model.OverrideBody, model.BodyHtml, chapterEmail?.BodyHtml);
        var subject = Resolve(model.OverrideSubject, model.Subject, chapterEmail?.Subject);

        /* Which fields this save actually writes. Both what may be written and what is checked hang off the
           same answer, so the two cannot disagree about whether a field is being touched. */
        var writesSubject = WritesWording(subject, chapterEmail?.Subject);
        var writesBody = WritesWording(bodyHtml, chapterEmail?.BodyHtml);

        /* Without the feature a group may still stop customising - that is the state it would be in had it
           never customised at all - but not write wording. Refusing the save outright instead would strand a
           group that lost the feature with wording it could neither change nor remove.

           The form disables what it must, but that is presentation - this is what withholds it. */
        if (!CanOverrideEmails(ownerSubscriptionFeatures) && (writesSubject || writesBody))
        {
            return ServiceResult.Failure(NotPermitted);
        }

        chapterEmail ??= new ChapterEmail
        {
            ChapterId = chapter.Id,
            Type = type
        };

        chapterEmail.BodyHtml = bodyHtml;
        chapterEmail.Subject = subject;

        var validationResult = ValidateChapterEmail(
            type,
            writesSubject ? subject : null,
            writesBody ? bodyHtml : null);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        // Blanking both fields is how a group goes back to the standard email, so the row goes rather than
        // being kept as an override of nothing - which would still badge the email as customised.
        if (!chapterEmail.OverridesAnything())
        {
            if (!chapterEmail.IsDefault())
            {
                _unitOfWork.ChapterEmailRepository.Delete(chapterEmail);
                await _unitOfWork.SaveChanges();
            }

            return ServiceResult.Successful();
        }

        _unitOfWork.ChapterEmailRepository.Upsert(chapterEmail);
        await _unitOfWork.SaveChanges();

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
            OwnerSubscriptionFeatures(chapter.Id, _siteSubscriptionCooldown));

        // The form renders read-only without the feature, but that is presentation - this is what
        // withholds it. Anything already set is left alone and keeps being used.
        if (!CanOverrideEmails(ownerSubscriptionFeatures))
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
        await _unitOfWork.SaveChanges();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UpdateEmail(IMemberServiceRequest request, EmailType type, EmailUpdateModel model)
    {
        var existing = await GetSiteAdminRestrictedContent(request,
            x => x.EmailRepository.GetByType(type));

        existing.BodyHtml = model.BodyHtml;
        existing.IsGroupEmail = model.IsGroupEmail;
        existing.Subject = model.Subject;

        var validationResult = ValidateEmail(existing);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        _unitOfWork.EmailRepository.Update(existing);
        await _unitOfWork.SaveChanges();

        return ServiceResult.Successful();
    }

    /* Authorised but deliberately not gated on the custom emails feature or on IsGroupEmail, unlike
       UpdateChapterEmail. Those refusals are about whether a template may be saved at all, and the form
       is read-only when either applies, so no request gets this far; answering them here would put
       "This email cannot be customised" under a field as if it were a markup error. */
    public async Task<ServiceResult> ValidateChapterEmailHtml(
        IMemberChapterAdminServiceRequest request,
        EmailType type,
        string? bodyHtml)
    {
        await AssertMemberIsChapterAdmin(request);

        return ValidateHtml(type, bodyHtml);
    }

    public ServiceResult ValidateEmailHtml(IMemberServiceRequest request, EmailType type, string? bodyHtml)
    {
        AssertMemberIsSiteAdmin(request.CurrentMember);

        return ValidateHtml(type, bodyHtml);
    }

    /* Batched into whichever query the caller is already running rather than fetched on its own, so
       reading the feature costs no extra round trip. */
    private static Func<IUnitOfWork, IDeferredQueryMultiple<SiteSubscriptionFeature>> OwnerSubscriptionFeatures(
        Guid chapterId, SiteSubscriptionCooldown cooldown)
        => x => x.MemberSiteSubscriptionRecordRepository
            .Query(x => x.Current().ForChapterOwner(chapterId).Active(cooldown))
            .SiteSubscription()
            .Features()
            .GetAll();

    /* The offered list is passed in rather than derived from the type: a group is offered fewer parameters
       than the site is, and the table has to describe what its reader can actually use.

       resolved holds whatever is already knowable while editing. The rest stand for the email being sent -
       the event, the member, the payment - and have no value until there is one. */
    private static IReadOnlyCollection<EmailParameterViewModel> Parameters(
        IReadOnlyCollection<string> names,
        string title,
        IReadOnlyDictionary<string, string>? resolved = null)
        => names
            .Select(x => new EmailParameterViewModel
            {
                Name = x,
                Description = EmailParameterDescriptions.For(x),
                Value = x == EmailParameters.TitleName
                    ? title
                    : resolved?.GetValueOrDefault(x)
            })
            .ToArray();

    /* What a field is left holding, given whether the group says it overrides it. Not overriding clears it.
       Overriding stores the wording supplied - or keeps what is already there where none is, which is how a
       field the form locked survives a save: a locked field posts nothing, and reading that as "cleared"
       would wipe wording the group never touched. */
    private static string? Resolve(bool overrides, string? supplied, string? stored)
    {
        if (!overrides)
        {
            return null;
        }

        return Unset(supplied) ?? stored;
    }

    private static string? Unset(string? value) => !string.IsNullOrWhiteSpace(value) ? value : null;

    /* Whether a field arrives carrying wording that is not already stored. Clearing it, or posting back what
       is there, is not writing - which is what lets a group without the feature save an override away. Both
       values have been through Unset, so null means the field is not overridden. */
    private static bool WritesWording(string? value, string? stored) => value != null && value != stored;

    /* Checked against everything the type supplies, not the narrower list a group is offered: a group
       template using platform.baseurl still resolves, so rejecting it would fail a working email. */
    private static ServiceResult ValidatePlaceholders(EmailType type, string subject, string bodyHtml)
    {
        var supplied = EmailTemplateParameters.ForSite(type);

        var unknown = EmailTemplateValidator.UnknownPlaceholders(subject, supplied)
            .Concat(EmailTemplateValidator.UnknownPlaceholders(bodyHtml, supplied))
            .Distinct(EmailParameterComparer.Default)
            .ToArray();

        return unknown.Length > 0
            ? ServiceResult.Failure(
                $"Unknown placeholder{(unknown.Length > 1 ? "s" : "")}: " +
                string.Join(", ", unknown.Select(x => $"{{{x}}}")))
            : ServiceResult.Successful();
    }

    private bool CanOverrideEmails(IReadOnlyCollection<SiteSubscriptionFeature> ownerSubscriptionFeatures)
        => _authorizationService.ChapterHasAccess(ownerSubscriptionFeatures, SiteFeatureType.CustomEmails);

    /* Takes only the wording this save writes; null for a field it leaves alone. A field the group is not
       writing was accepted when it was written, or predates the rule being applied now, so checking it again
       can only report a problem the group has no way to act on - and none at all where it is a field they
       cannot edit. That would let one field's stored wording block an unrelated change to the other.

       An unset field is not the group's wording either, so the same applies: holding it to these rules would
       report a problem with the site's template against a form the group cannot fix. */
    private ServiceResult ValidateChapterEmail(EmailType type, string? subject, string? bodyHtml)
    {
        if (!Enum.IsDefined(typeof(EmailType), type) || type == EmailType.None)
        {
            return ServiceResult.Failure("Invalid type");
        }

        var placeholderResult = ValidatePlaceholders(
            type, subject ?? string.Empty, bodyHtml ?? string.Empty);

        if (!placeholderResult.Success)
        {
            return placeholderResult;
        }

        return string.IsNullOrWhiteSpace(bodyHtml)
            ? ServiceResult.Successful()
            : ValidateHtml(type, bodyHtml);
    }

    private ServiceResult ValidateEmail(Email email)
    {
        if (!Enum.IsDefined(typeof(EmailType), email.Type) || email.Type == EmailType.None)
        {
            return ServiceResult.Failure("Invalid type");
        }

        if (string.IsNullOrWhiteSpace(email.BodyHtml) ||
            string.IsNullOrWhiteSpace(email.Subject))
        {
            return ServiceResult.Failure("Some required fields are missing");
        }

        var placeholderResult = ValidatePlaceholders(email.Type, email.Subject, email.BodyHtml);

        return !placeholderResult.Success
            ? placeholderResult
            : ValidateHtml(email.Type, email.BodyHtml);
    }


    /* The layout is exempt. It is the full HTML document every other email is rendered into -
       <html>, <head>, a stylesheet - so the allow-list tuned for rich text would reject it outright.
       Only the site admin edits it, and it is not a group email. Subjects are not checked
       either: they are plain text, so a stray angle bracket is not markup. */
    private ServiceResult ValidateHtml(EmailType type, string? bodyHtml) => type == EmailType.Layout
        ? ServiceResult.Successful()
        : _htmlValidator.Validate(bodyHtml, TemplateHtmlOptions);
}