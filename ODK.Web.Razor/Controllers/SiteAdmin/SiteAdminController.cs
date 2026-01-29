using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Payments;
using ODK.Services.Authentication;
using ODK.Services.Contact;
using ODK.Services.Events;
using ODK.Services.Features;
using ODK.Services.Logging;
using ODK.Services.Payments;
using ODK.Services.Settings;
using ODK.Services.SocialMedia;
using ODK.Services.Subscriptions;
using ODK.Services.Topics;
using ODK.Services.Topics.Models;
using ODK.Web.Common.Feedback;
using ODK.Web.Common.Routes;
using ODK.Web.Common.Services;
using ODK.Web.Razor.Models.Admin.Chapters;
using ODK.Web.Razor.Models.SiteAdmin;

namespace ODK.Web.Razor.Controllers.SiteAdmin;

[Authorize(Roles = OdkRoles.SiteAdmin)]
public class SiteAdminController : OdkControllerBase
{
    private readonly IContactAdminService _contactAdminService;
    private readonly IEventAdminService _eventAdminService;
    private readonly IFeatureService _featureService;
    private readonly ILoggingService _loggingService;
    private readonly ISettingsService _settingsService;
    private readonly ISiteSubscriptionAdminService _siteSubscriptionAdminService;
    private readonly ISocialMediaService _socialMediaService;
    private readonly ITopicAdminService _topicAdminService;

    public SiteAdminController(
        ILoggingService loggingService,
        ISocialMediaService socialMediaService,
        ISettingsService settingsService,
        ISiteSubscriptionAdminService siteSubscriptionAdminService,
        IFeatureService featureService,
        IContactAdminService contactAdminService,
        ITopicAdminService topicAdminService,
        IPaymentAdminService paymentAdminService,
        IRequestStore requestStore,
        IEventAdminService eventAdminService,
        IOdkRoutes odkRoutes)
        : base(requestStore, odkRoutes)
    {
        _contactAdminService = contactAdminService;
        _eventAdminService = eventAdminService;
        _featureService = featureService;
        _loggingService = loggingService;
        _settingsService = settingsService;
        _siteSubscriptionAdminService = siteSubscriptionAdminService;
        _socialMediaService = socialMediaService;
        _topicAdminService = topicAdminService;
    }

    [HttpGet("siteadmin")]
    public IActionResult Index()
    {
        return Redirect(OdkRoutes.SiteAdmin.Groups);
    }

    [HttpPost("siteadmin/errors/{id:guid}/delete")]
    public async Task<IActionResult> DeleteError(Guid id)
    {
        await _loggingService.DeleteError(MemberId, id);

        return Redirect(OdkRoutes.SiteAdmin.Errors);
    }

    [HttpPost("siteadmin/errors/{id:Guid}/deleteall")]
    public async Task<IActionResult> DeleteAllErrors(Guid id)
    {
        await _loggingService.DeleteAllErrors(MemberId, id);

        return Redirect(OdkRoutes.SiteAdmin.Errors);
    }

    [HttpPost("siteadmin/events/shortcodes")]
    public async Task<IActionResult> SetMissingEventShortcodes()
    {
        await _eventAdminService.SetMissingEventShortcodes(MemberServiceRequest);
        return RedirectToReferrer();
    }

    [HttpPost("siteadmin/features/{id:guid}/delete")]
    public async Task<IActionResult> DeleteFeature(Guid id)
    {
        await _featureService.DeleteFeature(MemberId, id);
        return Redirect(OdkRoutes.SiteAdmin.Features);
    }

    [HttpPost("siteadmin/messages/{id:guid}/replied")]
    public async Task<IActionResult> MarkMessageAsReplied(Guid id)
    {
        var result = await _contactAdminService.SetMessageAsReplied(MemberId, id);
        AddFeedback(result, "Message updated");
        return RedirectToReferrer();
    }

    [HttpPost("siteadmin/messages/{id:guid}/reply")]
    public async Task<IActionResult> ReplyToMessage(Guid id,
        [FromForm] ChapterMessageReplyFormViewModel viewModel)
    {
        var result = await _contactAdminService.ReplyToMessage(MemberServiceRequest, id, viewModel.Message ?? string.Empty);
        AddFeedback(result, "Reply sent");
        return RedirectToReferrer();
    }

    [HttpPost("siteadmin/payments")]
    public async Task<IActionResult> CreatePaymentSettings([FromForm] SitePaymentSettingsFormViewModel viewModel)
    {
        var result = await _settingsService.CreatePaymentSettings(MemberId,
            viewModel.Provider ?? PaymentProviderType.None,
            viewModel.Name ?? string.Empty,
            viewModel.PublicKey ?? string.Empty,
            viewModel.SecretKey ?? string.Empty,
            viewModel.Commission / 100,
            enabled: viewModel.Enabled);

        AddFeedback(result, "Payment settings created");

        return Redirect(OdkRoutes.SiteAdmin.Payments);
    }

    [HttpPost("siteadmin/payments/{id:guid}")]
    public async Task<IActionResult> UpdatePaymentSettings(Guid id,
        [FromForm] SitePaymentSettingsFormViewModel viewModel)
    {
        var result = await _settingsService.UpdatePaymentSettings(MemberId,
            id,
            viewModel.Name ?? string.Empty,
            viewModel.PublicKey ?? string.Empty,
            viewModel.SecretKey ?? string.Empty,
            viewModel.Commission / 100,
            enabled: viewModel.Enabled);

        AddFeedback(result, "Payment settings updated");

        return RedirectToReferrer();
    }

    [HttpPost("siteadmin/payments/{id:guid}/activate")]
    public async Task<IActionResult> ActivatePaymentSettings(Guid id)
    {
        var result = await _settingsService.ActivatePaymentSettings(MemberId, id);

        AddFeedback(result, "Active payment settings updated");

        return RedirectToReferrer();
    }

    [HttpPost("siteadmin/subscriptions")]
    public async Task<IActionResult> CreateSubscription(SiteSubscriptionFormViewModel viewModel)
    {
        var result = await _siteSubscriptionAdminService.AddSiteSubscription(MemberServiceRequest, new SiteSubscriptionCreateModel
        {
            Description = viewModel.Description,
            Name = viewModel.Name,
            Enabled = viewModel.Enabled,
            FallbackSiteSubscriptionId = viewModel.FallbackSiteSubscriptionId,
            Features = viewModel.Features,
            GroupLimit = viewModel.GroupLimit,
            MemberLimit = viewModel.MemberLimit,
            SitePaymentSettingId = viewModel.SitePaymentSettingId ?? Guid.Empty
        });

        AddFeedback(result, "Subscription created");

        return Redirect(OdkRoutes.SiteAdmin.Subscriptions);
    }

    [HttpPost("siteadmin/subscriptions/{id:guid}")]
    public async Task<IActionResult> UpdateSubscription(Guid id, SiteSubscriptionFormViewModel viewModel)
    {
        var result = await _siteSubscriptionAdminService.UpdateSiteSubscription(MemberServiceRequest, id, new SiteSubscriptionCreateModel
        {
            Description = viewModel.Description,
            Name = viewModel.Name,
            Enabled = viewModel.Enabled,
            FallbackSiteSubscriptionId = viewModel.FallbackSiteSubscriptionId,
            Features = viewModel.Features,
            GroupLimit = viewModel.GroupLimit,
            MemberLimit = viewModel.MemberLimit,
            SitePaymentSettingId = viewModel.SitePaymentSettingId ?? Guid.Empty
        });

        if (result.Success)
        {
            AddFeedback("Subscription updated", FeedbackType.Success);
            return Redirect(OdkRoutes.SiteAdmin.Subscriptions);
        }

        return RedirectToReferrer();
    }

    [HttpPost("siteadmin/subscriptions/{id:guid}/default")]
    public async Task<IActionResult> MakeDefault(Guid id)
    {
        await _siteSubscriptionAdminService.MakeDefault(MemberServiceRequest, id);
        AddFeedback("Default subscription updated", FeedbackType.Success);
        return RedirectToReferrer();
    }

    [HttpPost("siteadmin/subscriptions/{id:guid}/disable")]
    public async Task<IActionResult> DisableSubscription(Guid id)
    {
        await _siteSubscriptionAdminService.UpdateSiteSubscriptionEnabled(MemberId, id, false);
        AddFeedback("Subscription disabled", FeedbackType.Success);
        return RedirectToReferrer();
    }

    [HttpPost("siteadmin/subscriptions/{id:guid}/enable")]
    public async Task<IActionResult> EnableSubscription(Guid id)
    {
        await _siteSubscriptionAdminService.UpdateSiteSubscriptionEnabled(MemberId, id, true);
        AddFeedback("Subscription enabled", FeedbackType.Success);
        return RedirectToReferrer();
    }

    [HttpPost("siteadmin/subscriptions/{id:guid}/prices")]
    public async Task<IActionResult> AddSiteSubscriptionPrice(Guid id,
        SiteSubscriptionPriceFormViewModel viewModel)
    {
        var result = await _siteSubscriptionAdminService.AddSiteSubscriptionPrice(MemberServiceRequest, id, new SiteSubscriptionPriceCreateModel
        {
            Amount = viewModel.Amount ?? default,
            CurrencyId = viewModel.CurrencyId ?? default,
            Frequency = viewModel.Frequency ?? default
        });

        if (result.Success)
        {
            AddFeedback("Subscription price added", FeedbackType.Success);
        }
        else
        {
            AddFeedback(result);
        }

        return RedirectToReferrer();
    }

    [HttpPost("siteadmin/subscriptions/{siteSubscriptionId:guid}/Prices/{id:guid}/Delete")]
    public async Task<IActionResult> DeleteSiteSubscriptionPrice(Guid siteSubscriptionId, Guid id)
    {
        await _siteSubscriptionAdminService.DeleteSiteSubscriptionPrice(MemberId, siteSubscriptionId, id);
        AddFeedback("Subscription price deleted", FeedbackType.Success);
        return RedirectToReferrer();
    }

    [HttpPost("siteadmin/topic-groups")]
    public async Task<IActionResult> AddTopicGroup([FromForm] string name)
    {
        var result = await _topicAdminService.AddTopicGroup(MemberId, name);
        AddFeedback(result, "Topic group added");
        return RedirectToReferrer();
    }

    [HttpPost("siteadmin/topics")]
    public async Task<IActionResult> AddTopic([FromForm] Guid topicGroupId, [FromForm] string name)
    {
        var result = await _topicAdminService.AddTopic(MemberId, topicGroupId, name);
        AddFeedback(result, "Topic added");
        return RedirectToReferrer();
    }

    [HttpPost("siteadmin/topics/approve")]
    public async Task<IActionResult> ApproveTopics(NewTopicsFormViewModel viewModel)
    {
        var approved = new ApproveTopicsModel
        {
            Chapters = viewModel.Chapters?
                .Where(x => x.Approved && !x.Rejected)
                .Select(x => new ApproveTopicsItemModel
                {
                    NewTopicId = x.NewTopicId,
                    Topic = x.Topic,
                    TopicGroup = x.TopicGroup
                })
                .ToArray() ?? [],
            Members = viewModel.Members?
                .Where(x => x.Approved && !x.Rejected)
                .Select(x => new ApproveTopicsItemModel
                {
                    NewTopicId = x.NewTopicId,
                    Topic = x.Topic,
                    TopicGroup = x.TopicGroup
                })
                .ToArray() ?? []
        };

        var rejected = new ApproveTopicsModel
        {
            Chapters = viewModel.Chapters?
                .Where(x => x.Rejected && !x.Approved)
                .Select(x => new ApproveTopicsItemModel
                {
                    NewTopicId = x.NewTopicId,
                    Topic = x.Topic,
                    TopicGroup = x.TopicGroup
                })
                .ToArray() ?? [],
            Members = viewModel.Members?
                .Where(x => x.Rejected && !x.Approved)
                .Select(x => new ApproveTopicsItemModel
                {
                    NewTopicId = x.NewTopicId,
                    Topic = x.Topic,
                    TopicGroup = x.TopicGroup
                })
                .ToArray() ?? []
        };

        await _topicAdminService.ApproveTopics(MemberServiceRequest, approved, rejected);

        AddFeedback("Topics processed", FeedbackType.Success);

        return RedirectToReferrer();
    }

    [HttpPost("siteadmin/topics/{id:guid}")]
    public async Task<IActionResult> UpdateTopic(Guid id, [FromForm] TopicFormSubmitViewModel viewModel)
    {
        var result = await _topicAdminService.UpdateTopic(MemberServiceRequest, id, new UpdateTopicModel
        {
            TopicGroupId = viewModel.TopicGroupId
        });

        AddFeedback(result, "Topic updated");

        return Redirect(OdkRoutes.SiteAdmin.Topics);
    }

    [HttpGet("{chapterName}/Admin/siteadmin")]
    public IActionResult Index(string chapterName)
    {
        return Redirect($"/{chapterName}/admin/siteadmin/payments");
    }

    [HttpPost("groups/{chapterId:guid}/siteadmin/instagram/scrape")]
    public async Task<IActionResult> ScrapeInstagram(Guid chapterId)
    {
        var result = await _socialMediaService.ScrapeLatestInstagramPosts(chapterId);
        AddFeedback(result);
        return RedirectToReferrer();
    }
}