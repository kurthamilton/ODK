using Microsoft.AspNetCore.Mvc;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Services;
using ODK.Web.Common.Routes;
using ODK.Web.Common.Services;
using ODK.Web.Razor.Extensions;
using ODK.Web.Razor.Models.Feedback;

namespace ODK.Web.Razor.Controllers;

public abstract class OdkControllerBase : Controller
{
    protected OdkControllerBase(
        IRequestStore requestStore,
        IOdkRoutes odkRoutes)
    {
        RequestStore = requestStore;
        OdkRoutes = odkRoutes;
    }

    protected Chapter Chapter => RequestStore.Chapter;

    protected IChapterServiceRequest ChapterServiceRequest => RequestStore.ChapterServiceRequest;

    protected Member CurrentMember => RequestStore.CurrentMember;

    protected IMemberChapterServiceRequest MemberChapterServiceRequest => RequestStore.MemberChapterServiceRequest;

    protected IMemberServiceRequest MemberServiceRequest => RequestStore.MemberServiceRequest;

    protected IOdkRoutes OdkRoutes { get; }

    protected PlatformType Platform => RequestStore.Platform;

    protected IRequestStore RequestStore { get; }

    protected IServiceRequest ServiceRequest => RequestStore.ServiceRequest;

    protected void AddFeedback(string message, FeedbackType type)
        => AddFeedback(new FeedbackViewModel(message, type));

    protected void AddFeedback(ServiceResult result)
        => AddFeedback(FeedbackViewModel.FromResult(result));

    protected void AddFeedback(ServiceResult result, string successMessage)
        => AddFeedback(FeedbackViewModel.FromResult(result, successMessage));

    protected void AddFeedback(IReadOnlyCollection<FeedbackViewModel> viewModels)
    {
        foreach (var viewModel in viewModels)
        {
            AddFeedback(viewModel);
        }
    }

    protected void AddFeedback(FeedbackViewModel viewModel)
    {
        TempData!.AddFeedback(viewModel);
    }

    protected IActionResult CacheableFile(byte[] data, string mimeType, int? version)
    {
        // Do not set cache control if no version was given for the image
        if (version != null)
        {
            Response.Headers.CacheControl = "public, max-age=31536000, immutable";
        }

        return File(data, mimeType);
    }

    /// <summary>
    /// The result of a post the page made by script: the feedback it has to show, as the response to that
    /// post. Deliberately not TempData - the post does not redirect, so feedback left there would surface on
    /// whichever page the member loaded next.
    /// </summary>
    protected IActionResult FeedbackResponse(ServiceResult result)
        => FeedbackResponse(FeedbackViewModel.FromResult(result));

    /// <inheritdoc cref="FeedbackResponse(ServiceResult)"/>
    protected IActionResult FeedbackResponse(ServiceResult result, string successMessage)
        => FeedbackResponse(FeedbackViewModel.FromResult(result, successMessage));

    /// <inheritdoc cref="FeedbackResponse(ServiceResult)"/>
    protected IActionResult FeedbackResponse(IReadOnlyCollection<FeedbackViewModel> viewModels)
        => Ok(new FeedbackResponseViewModel
        {
            Feedback = viewModels
        });

    protected string? GetHeader(string name)
        => Request.Headers
            .GetCommaSeparatedValues(name)
            .FirstOrDefault();

    protected async Task<string> ReadBodyText()
    {
        using var reader = new StreamReader(Request.Body);
        var text = await reader.ReadToEndAsync();
        return text;
    }

    protected IActionResult RedirectToReferrer(string? fallback = null)
    {
        var referer = Request.LocalRefererOrDefault();
        if (referer != null)
        {
            return Redirect(referer);
        }

        return Redirect(!string.IsNullOrEmpty(fallback) ? fallback : Request.Path);
    }

    /// <summary>
    /// Redirects to <paramref name="returnUrl"/> only when it is a local URL, otherwise to
    /// <paramref name="fallback"/>. Centralises the open-redirect guard so callers can't forget it.
    /// </summary>
    protected IActionResult RedirectToReturnUrl(string? returnUrl, string fallback)
        => !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? Redirect(returnUrl)
            : Redirect(fallback);

}