using Microsoft.AspNetCore.Mvc;
using ODK.Services.Caching;
using ODK.Services.Contact;
using ODK.Services.Emails;
using ODK.Services.Features;
using ODK.Services.Logging;
using ODK.Services.Settings;
using ODK.Services.SocialMedia;
using ODK.Services.Subscriptions;
using ODK.Services.Topics;
using ODK.Services.Topics.Models;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.Admin.Chapters;
using ODK.Web.Razor.Models.SuperAdmin;

namespace ODK.Web.Razor.Controllers.SuperAdmin;

public class SuperAdminController : OdkControllerBase
{
    private readonly IContactAdminService _contactAdminService;
    private readonly IEmailAdminService _emailAdminService;
    private readonly IFeatureService _featureService;
    private readonly IInstagramService _instagramService;
    private readonly ILoggingService _loggingService;
    private readonly IRequestCache _requestCache;
    private readonly ISettingsService _settingsService;
    private readonly ISiteSubscriptionAdminService _siteSubscriptionAdminService;
    private readonly ITopicAdminService _topicAdminService;

    public SuperAdminController(
        IEmailAdminService emailAdminService,
        ILoggingService loggingService, 
        IInstagramService instagramService,
        IRequestCache requestCache, 
        ISettingsService settingsService,
        ISiteSubscriptionAdminService siteSubscriptionAdminService,
        IFeatureService featureService,
        IContactAdminService contactAdminService,
        ITopicAdminService topicAdminService)
    {
        _contactAdminService = contactAdminService;
        _emailAdminService = emailAdminService;
        _featureService = featureService;
        _instagramService = instagramService;
        _loggingService = loggingService;
        _requestCache = requestCache;
        _settingsService = settingsService;
        _siteSubscriptionAdminService = siteSubscriptionAdminService;
        _topicAdminService = topicAdminService;
    }

    [HttpGet("superadmin")]
    public IActionResult Index()
    {
        return Redirect("/superadmin/emails");
    }

    [HttpPost("superadmin/errors/{id:guid}/delete")]
    public async Task<IActionResult> DeleteError(Guid id)
    {
        await _loggingService.DeleteError(MemberId, id);

        return Redirect("/superadmin/errors");
    }

    [HttpPost("superadmin/errors/{id:Guid}/deleteall")]
    public async Task<IActionResult> DeleteAllErrors(Guid id)
    {
        await _loggingService.DeleteAllErrors(MemberId, id);

        return Redirect("/superAdmin/errors");
    }

    [HttpPost("superadmin/features/{id:guid}/delete")]
    public async Task<IActionResult> DeleteFeature(Guid id)
    {
        await _featureService.DeleteFeature(MemberId, id);
        return Redirect("/superadmin/features");
    }

    [HttpPost("superadmin/messages/{id:guid}/replied")]
    public async Task<IActionResult> MarkMessageAsReplied(Guid id)
    {
        var result = await _contactAdminService.SetMessageAsReplied(MemberId, id);
        AddFeedback(result, "Message updated");
        return RedirectToReferrer();
    }

    [HttpPost("superadmin/messages/{id:guid}/reply")]
    public async Task<IActionResult> ReplyToMessage(Guid id,
        [FromForm] ChapterMessageReplyFormViewModel viewModel)
    {
        var result = await _contactAdminService.ReplyToMessage(MemberId, id, viewModel.Message ?? "");
        AddFeedback(result, "Reply sent");
        return RedirectToReferrer();
    }

    [HttpPost("superadmin/subscriptions")]
    public async Task<IActionResult> CreateSubscription(SiteSubscriptionFormViewModel viewModel)
    {
        var result = await _siteSubscriptionAdminService.AddSiteSubscription(MemberId, new SiteSubscriptionCreateModel
        {
            Description = viewModel.Description,
            Name = viewModel.Name,
            Enabled = viewModel.Enabled,
            FallbackSiteSubscriptionId = viewModel.FallbackSiteSubscriptionId,
            GroupLimit = viewModel.GroupLimit,
            MemberLimit = viewModel.MemberLimit,
            MemberSubscriptions = viewModel.MemberSubscriptions,
            Premium = viewModel.Premium,
            SendMemberEmails = viewModel.SendMemberEmails
        });

        if (result.Success)
        {
            AddFeedback("Subscription created", FeedbackType.Success);
        }

        return Redirect("/superadmin/subscriptions");
    }

    [HttpPost("superadmin/subscriptions/{id:guid}")]
    public async Task<IActionResult> UpdateSubscription(Guid id, SiteSubscriptionFormViewModel viewModel)
    {
        var result = await _siteSubscriptionAdminService.UpdateSiteSubscription(MemberId, id, new SiteSubscriptionCreateModel
        {
            Description = viewModel.Description,
            Name = viewModel.Name,
            Enabled = viewModel.Enabled,
            FallbackSiteSubscriptionId = viewModel.FallbackSiteSubscriptionId,
            GroupLimit = viewModel.GroupLimit,
            MemberLimit = viewModel.MemberLimit,
            MemberSubscriptions = viewModel.MemberSubscriptions,
            Premium = viewModel.Premium,
            SendMemberEmails = viewModel.SendMemberEmails
        });

        if (result.Success)
        {
            AddFeedback("Subscription updated", FeedbackType.Success);
        }

        return RedirectToReferrer();
    }

    [HttpPost("superadmin/subscriptions/{id:guid}/default")]
    public async Task<IActionResult> MakeDefault(Guid id)
    {
        await _siteSubscriptionAdminService.MakeDefault(MemberId, id);
        AddFeedback("Default subscription updated", FeedbackType.Success);
        return RedirectToReferrer();
    }

    [HttpPost("superadmin/subscriptions/{id:guid}/prices")]
    public async Task<IActionResult> AddSiteSubscriptionPrice(Guid id, 
        SiteSubscriptionPriceFormViewModel viewModel)
    {
        var result = await _siteSubscriptionAdminService.AddSiteSubscriptionPrice(MemberId, id, new SiteSubscriptionPriceCreateModel
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

    [HttpPost("SuperAdmin/Subscriptions/{siteSubscriptionId:guid}/Prices/{id:guid}/Delete")]
    public async Task<IActionResult> DeleteSiteSubscriptionPrice(Guid siteSubscriptionId, Guid id)
    {
        await _siteSubscriptionAdminService.DeleteSiteSubscriptionPrice(MemberId, siteSubscriptionId, id);
        AddFeedback("Subscription price deleted", FeedbackType.Success);
        return RedirectToReferrer();
    }

    [HttpPost("superadmin/topics")]
    public async Task<IActionResult> AddTopic([FromForm] Guid topicGroupId, [FromForm] string name)
    {
        var result = await _topicAdminService.AddTopic(MemberId, topicGroupId, name);
        AddFeedback(result, "Topic added");
        return RedirectToReferrer();
    }

    [HttpPost("superadmin/topics/approve")]
    public async Task<IActionResult> ApproveTopics(NewTopicsFormViewModel viewModel)
    {
        var approved = new ApproveTopicsModel
        {
            Chapters = viewModel.Chapters
                .Where(x => x.Approved && !x.Rejected)
                .Select(x => new ApproveTopicsItemModel
                {
                    NewTopicId = x.NewTopicId,
                    Topic = x.Topic,
                    TopicGroup = x.TopicGroup
                })
                .ToArray(),
            Members = viewModel.Members
                .Where(x => x.Approved && !x.Rejected)
                .Select(x => new ApproveTopicsItemModel
                {
                    NewTopicId = x.NewTopicId,
                    Topic = x.Topic,
                    TopicGroup = x.TopicGroup
                })
                .ToArray()
        };

        var rejected = new ApproveTopicsModel
        {
            Chapters = viewModel.Chapters
                .Where(x => x.Rejected && !x.Approved)
                .Select(x => new ApproveTopicsItemModel
                {
                    NewTopicId = x.NewTopicId,
                    Topic = x.Topic,
                    TopicGroup = x.TopicGroup
                })
                .ToArray(),
            Members = viewModel.Members
                .Where(x => x.Rejected && !x.Approved)
                .Select(x => new ApproveTopicsItemModel
                {
                    NewTopicId = x.NewTopicId,
                    Topic = x.Topic,
                    TopicGroup = x.TopicGroup
                })
                .ToArray()
        };

        await _topicAdminService.ApproveTopics(MemberId, approved, rejected);

        AddFeedback("Topics processed", FeedbackType.Success);

        return RedirectToReferrer();
    }

    [HttpGet("{chapterName}/Admin/SuperAdmin")]
    public IActionResult Index(string chapterName)
    {
        return Redirect($"/{chapterName}/admin/superadmin/payments");
    }

    [HttpPost("{chapterName}/Admin/SuperAdmin/Instagram/Scrape")]
    public async Task<IActionResult> ScrapeInstagram(string chapterName)
    {
        var chapter = await _requestCache.GetChapterAsync(chapterName);
        if (chapter == null)
        {
            return RedirectToReferrer();
        }

        await _instagramService.ScrapeLatestInstagramPosts(chapter.Id);

        return RedirectToReferrer();
    }
}
