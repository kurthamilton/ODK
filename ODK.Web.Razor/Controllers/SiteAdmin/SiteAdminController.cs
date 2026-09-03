using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Services.Authentication;
using ODK.Services.Contact;
using ODK.Services.Features;
using ODK.Services.Logging;
using ODK.Services.Payments;
using ODK.Services.Payments.Models;
using ODK.Services.SocialMedia;
using ODK.Services.Subscriptions;
using ODK.Services.Subscriptions.Models;
using ODK.Services.Topics;
using ODK.Services.Topics.Models;
using ODK.Web.Common.Routes;
using ODK.Web.Common.Services;
using ODK.Web.Razor.Models.Admin.Chapters;
using ODK.Web.Razor.Models.Chapters;
using ODK.Web.Razor.Models.Feedback;
using ODK.Web.Razor.Models.SiteAdmin;

namespace ODK.Web.Razor.Controllers.SiteAdmin;

[Authorize(Roles = OdkRoles.SiteAdmin)]
public class SiteAdminController : OdkControllerBase
{
    private readonly IContactAdminService _contactAdminService;
    private readonly IFeatureService _featureService;
    private readonly ILoggingService _loggingService;
    private readonly IPaymentAdminService _paymentAdminService;
    private readonly ISiteSubscriptionAdminService _siteSubscriptionAdminService;
    private readonly ISocialMediaService _socialMediaService;
    private readonly ITopicAdminService _topicAdminService;

    public SiteAdminController(
        ILoggingService loggingService,
        ISocialMediaService socialMediaService,
        ISiteSubscriptionAdminService siteSubscriptionAdminService,
        IFeatureService featureService,
        IContactAdminService contactAdminService,
        IPaymentAdminService paymentAdminService,
        ITopicAdminService topicAdminService,
        IRequestStore requestStore,
        IOdkRoutes odkRoutes)
        : base(requestStore, odkRoutes)
    {
        _contactAdminService = contactAdminService;
        _featureService = featureService;
        _loggingService = loggingService;
        _paymentAdminService = paymentAdminService;
        _siteSubscriptionAdminService = siteSubscriptionAdminService;
        _socialMediaService = socialMediaService;
        _topicAdminService = topicAdminService;
    }

    [HttpGet("siteadmin")]
    public IActionResult Index()
    {
        return Redirect(OdkRoutes.SiteAdmin.Groups.Path);
    }

    [HttpPost("siteadmin/errors/{id:guid}/delete")]
    public async Task<IActionResult> DeleteError(Guid id)
    {
        await _loggingService.DeleteError(MemberServiceRequest, id);

        return Redirect(OdkRoutes.SiteAdmin.Errors.Path);
    }

    [HttpPost("siteadmin/errors/{id:Guid}/deleteall")]
    public async Task<IActionResult> DeleteAllErrors(Guid id)
    {
        await _loggingService.DeleteAllErrors(MemberServiceRequest, id);

        return Redirect(OdkRoutes.SiteAdmin.Errors.Path);
    }

    [HttpPost("siteadmin/features/{id:guid}/delete")]
    public async Task<IActionResult> DeleteFeature(Guid id)
    {
        await _featureService.DeleteFeature(MemberServiceRequest, id);
        return Redirect(OdkRoutes.SiteAdmin.Features.Path);
    }

    [HttpPost("siteadmin/messages/{id:guid}/replied")]
    public async Task<IActionResult> MarkMessageAsReplied(Guid id)
    {
        var result = await _contactAdminService.SetMessageAsReplied(MemberServiceRequest, id);
        AddFeedback(result, "Message updated");
        return RedirectToReferrer();
    }

    [HttpPost("siteadmin/conversations/{id:guid}/reply")]
    public async Task<IActionResult> ReplyToSiteConversation(Guid id,
        [FromForm] ChapterConversationReplyFormViewModel viewModel)
    {
        var result = await _contactAdminService.ReplyToSiteConversation(
            MemberServiceRequest, id, viewModel.Message ?? string.Empty);
        AddFeedback(result, "Reply sent");
        return RedirectToReferrer();
    }

    [HttpPost("siteadmin/messages/{id:guid}/reply")]
    public async Task<IActionResult> ReplyToMessage(Guid id,
        [FromForm] ChapterMessageReplyFormViewModel viewModel)
    {
        var result = await _contactAdminService.ReplyToMessage(MemberServiceRequest, id, viewModel.MessageHtml ?? string.Empty);
        AddFeedback(result, "Reply sent");
        return RedirectToReferrer();
    }

    [HttpPost("siteadmin/messages/spam/delete")]
    public async Task<IActionResult> DeleteSpamMessages()
    {
        var result = await _contactAdminService.DeleteSpamMessages(MemberServiceRequest);
        AddFeedback(result, "Spam messages deleted");
        return RedirectToReferrer();
    }

    [HttpPost("siteadmin/payments/{id:guid}/reconcile/ignore")]
    public async Task<IActionResult> IgnorePayment(Guid id)
    {
        var result = await _paymentAdminService.IgnorePayment(MemberServiceRequest, id);

        AddFeedback(result);
        return Redirect(OdkRoutes.SiteAdmin.PaymentReconciliation.Path);
    }

    [HttpPost("siteadmin/payments/reconcile/ignore")]
    public async Task<IActionResult> IgnorePayments(
        [FromForm] PaymentReconciliationFormSubmitViewModel viewModel)
    {
        var result = await _paymentAdminService.IgnorePayments(
            MemberServiceRequest, viewModel.PaymentIds);

        AddFeedback(result);
        return Redirect(OdkRoutes.SiteAdmin.PaymentReconciliation.Path);
    }

    [HttpPost("siteadmin/payments/{id:guid}/reconcile/unignore")]
    public async Task<IActionResult> UnignorePayment(Guid id)
    {
        var result = await _paymentAdminService.UnignorePayment(MemberServiceRequest, id);

        AddFeedback(result);
        return Redirect(OdkRoutes.SiteAdmin.PaymentReconciliation.Path);
    }

    [HttpPost("siteadmin/payments/{id:guid}/reconcile")]
    public async Task<IActionResult> ReconcilePayment(Guid id)
    {
        var result = await _paymentAdminService.ReconcilePayment(MemberServiceRequest, id);

        AddFeedback(result);
        return Redirect(OdkRoutes.SiteAdmin.PaymentReconciliation.Path);
    }

    [HttpPost("siteadmin/payments/reconcile")]
    public async Task<IActionResult> ReconcilePayments(
        [FromForm] PaymentReconciliationFormSubmitViewModel viewModel)
    {
        var result = await _paymentAdminService.ReconcilePayments(
            MemberServiceRequest, viewModel.PaymentIds);

        AddFeedback(result);
        return Redirect(OdkRoutes.SiteAdmin.PaymentReconciliation.Path);
    }

    [HttpPost("siteadmin/payments/{id}/refund")]
    public async Task<IActionResult> RefundPayment(
        [FromForm] RefundPaymentFormSubmitViewModel viewModel, Guid id)
    {
        var result = await _paymentAdminService.RefundPayment(
            MemberServiceRequest,
            id,
            new RefundPaymentModel
            {
                Amount = viewModel.Amount,
                Reason = viewModel.Reason
            });

        AddFeedback(result);
        return RedirectToReferrer();
    }

    [HttpPost("siteadmin/subscriptions")]
    public async Task<IActionResult> CreateSubscription(SiteSubscriptionFormSubmitViewModel viewModel)
    {
        var result = await _siteSubscriptionAdminService.AddSiteSubscription(MemberServiceRequest, new SiteSubscriptionCreateModel
        {
            DescriptionHtml = viewModel.DescriptionHtml,
            Name = viewModel.Name,
            Enabled = viewModel.Enabled,
            FallbackSiteSubscriptionId = viewModel.FallbackSiteSubscriptionId,
            Features = viewModel.Features ?? [],
            Free = viewModel.Free,
            GroupLimit = viewModel.GroupLimit,
            MemberLimit = viewModel.MemberLimit
        });

        AddFeedback(result, "Subscription created");

        return Redirect(OdkRoutes.SiteAdmin.Subscription(result.Value).Path);
    }

    [HttpPost("siteadmin/subscriptions/{id:guid}")]
    public async Task<IActionResult> UpdateSubscription(Guid id, SiteSubscriptionFormSubmitViewModel viewModel)
    {
        var result = await _siteSubscriptionAdminService.UpdateSiteSubscription(MemberServiceRequest, id, new SiteSubscriptionCreateModel
        {
            DescriptionHtml = viewModel.DescriptionHtml,
            Name = viewModel.Name,
            Enabled = viewModel.Enabled,
            FallbackSiteSubscriptionId = viewModel.FallbackSiteSubscriptionId,
            Features = viewModel.Features ?? [],
            Free = viewModel.Free,
            GroupLimit = viewModel.GroupLimit,
            MemberLimit = viewModel.MemberLimit
        });

        if (result.Success)
        {
            AddFeedback("Subscription updated", FeedbackType.Success);
            return Redirect(OdkRoutes.SiteAdmin.Subscriptions.Path);
        }

        return RedirectToReferrer();
    }

    [HttpPost("siteadmin/subscriptions/{id:guid}/default")]
    public async Task<IActionResult> MakeDefault(Guid id)
    {
        var result = await _siteSubscriptionAdminService.MakeDefault(MemberServiceRequest, id);
        AddFeedback(result, "Default subscription updated");
        return RedirectToReferrer();
    }

    [HttpPost("siteadmin/subscriptions/{id:guid}/delete")]
    public async Task<IActionResult> DeleteSubscription(Guid id)
    {
        var result = await _siteSubscriptionAdminService.DeleteSiteSubscription(MemberServiceRequest, id);
        AddFeedback(result, "Subscription deleted");
        return Redirect(OdkRoutes.SiteAdmin.Subscriptions.Path);
    }

    [HttpPost("siteadmin/subscriptions/{id:guid}/disable")]
    public async Task<IActionResult> DisableSubscription(Guid id)
    {
        await _siteSubscriptionAdminService.UpdateSiteSubscriptionEnabled(MemberServiceRequest, id, false);
        AddFeedback("Subscription disabled", FeedbackType.Success);
        return RedirectToReferrer();
    }

    [HttpPost("siteadmin/subscriptions/{id:guid}/enable")]
    public async Task<IActionResult> EnableSubscription(Guid id)
    {
        await _siteSubscriptionAdminService.UpdateSiteSubscriptionEnabled(MemberServiceRequest, id, true);
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
        var result = await _siteSubscriptionAdminService.DeleteSiteSubscriptionPrice(
            MemberServiceRequest, siteSubscriptionId, id);
        AddFeedback(result, "Subscription price deleted");
        return RedirectToReferrer();
    }

    [HttpPost("siteadmin/topic-groups")]
    public async Task<IActionResult> AddTopicGroup([FromForm] string name)
    {
        var result = await _topicAdminService.AddTopicGroup(MemberServiceRequest, name);
        AddFeedback(result, "Topic group added");
        return RedirectToReferrer();
    }

    [HttpPost("siteadmin/topics")]
    public async Task<IActionResult> AddTopic([FromForm] Guid topicGroupId, [FromForm] string name)
    {
        var result = await _topicAdminService.AddTopic(MemberServiceRequest, topicGroupId, name);
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
        var result = await _topicAdminService.UpdateTopic(MemberServiceRequest, id, new TopicUpdateModel
        {
            TopicGroupId = viewModel.TopicGroupId
        });

        AddFeedback(result, "Topic updated");

        return Redirect(OdkRoutes.SiteAdmin.Topics.Path);
    }

    [HttpPost("groups/{chapterId:guid}/siteadmin/instagram/scrape")]
    public async Task<IActionResult> ScrapeInstagram(Guid chapterId)
    {
        var result = await _socialMediaService.ScrapeLatestInstagramPosts(chapterId);
        AddFeedback(result);
        return RedirectToReferrer();
    }
}