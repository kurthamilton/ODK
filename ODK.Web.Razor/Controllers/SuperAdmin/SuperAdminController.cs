﻿using Microsoft.AspNetCore.Mvc;
using ODK.Services.Caching;
using ODK.Services.Emails;
using ODK.Services.Logging;
using ODK.Services.Settings;
using ODK.Services.SocialMedia;
using ODK.Services.Subscriptions;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Models.SuperAdmin;

namespace ODK.Web.Razor.Controllers.SuperAdmin;

public class SuperAdminController : OdkControllerBase
{
    private readonly IEmailAdminService _emailAdminService;
    private readonly IInstagramService _instagramService;
    private readonly ILoggingService _loggingService;
    private readonly IRequestCache _requestCache;
    private readonly ISettingsService _settingsService;
    private readonly ISiteSubscriptionAdminService _siteSubscriptionAdminService;

    public SuperAdminController(IEmailAdminService emailAdminService,
        ILoggingService loggingService, IInstagramService instagramService,
        IRequestCache requestCache, ISettingsService settingsService,
        ISiteSubscriptionAdminService siteSubscriptionAdminService)
    {
        _emailAdminService = emailAdminService;
        _instagramService = instagramService;
        _loggingService = loggingService;
        _requestCache = requestCache;
        _settingsService = settingsService;
        _siteSubscriptionAdminService = siteSubscriptionAdminService;
    }

    [HttpGet("SuperAdmin")]
    public IActionResult Index()
    {
        return Redirect("/SuperAdmin/Emails");
    }

    [HttpPost("SuperAdmin/Errors/{id:guid}/Delete")]
    public async Task<IActionResult> DeleteError(Guid id)
    {
        await _loggingService.DeleteError(MemberId, id);

        return Redirect("/SuperAdmin/Errors");
    }

    [HttpPost("SuperAdmin/Errors/{id:Guid}/DeleteAll")]
    public async Task<IActionResult> DeleteAllErrors(Guid id)
    {
        await _loggingService.DeleteAllErrors(MemberId, id);

        return Redirect("/SuperAdmin/Errors");
    }

    [HttpPost("SuperAdmin/Subscriptions")]
    public async Task<IActionResult> CreateSubscription(SiteSubscriptionFormViewModel viewModel)
    {
        var result = await _siteSubscriptionAdminService.AddSiteSubscription(MemberId, new SiteSubscriptionCreateModel
        {
            Description = viewModel.Description,
            Name = viewModel.Name,
            Enabled = viewModel.Enabled,
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

        return Redirect("/SuperAdmin/Subscriptions");
    }

    [HttpPost("SuperAdmin/Subscriptions/{id:guid}")]
    public async Task<IActionResult> UpdateSubscription(Guid id, SiteSubscriptionFormViewModel viewModel)
    {
        var result = await _siteSubscriptionAdminService.UpdateSiteSubscription(MemberId, id, new SiteSubscriptionCreateModel
        {
            Description = viewModel.Description,
            Name = viewModel.Name,
            Enabled = viewModel.Enabled,
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

    [HttpPost("SuperAdmin/Subscriptions/{id:guid}/Prices")]
    public async Task<IActionResult> AddSiteSubscriptionPrice(Guid id, SiteSubscriptionPriceFormViewModel viewModel)
    {
        var result = await _siteSubscriptionAdminService.AddSiteSubscriptionPrice(MemberId, id, new SiteSubscriptionPriceCreateModel
        {
            CurrencyId = viewModel.CurrencyId ?? default,
            MonthlyAmount = viewModel.MonthlyAmount ?? default,
            YearlyAmount = viewModel.YearlyAmount ?? default
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


    [HttpGet("{chapterName}/Admin/SuperAdmin")]
    public IActionResult Index(string chapterName)
    {
        return Redirect($"/{chapterName}/Admin/SuperAdmin/Payments");
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
