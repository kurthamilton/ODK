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
        => AddFeedback(new FeedbackViewModel(result));

    protected void AddFeedback(ServiceResult result, string successMessage)
    {
        if (result.Success)
        {
            var message = !string.IsNullOrEmpty(result.Message) ? result.Message : successMessage;
            AddFeedback(message, FeedbackType.Success);
        }
        else
        {
            AddFeedback(result);
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
        var referer = Request.Headers.Referer.ToString();

        // Only follow the Referer when it points back to this host - an off-site Referer would be an
        // open-redirect vector.
        if (!string.IsNullOrEmpty(referer)
            && Uri.TryCreate(referer, UriKind.Absolute, out var refererUri)
            && string.Equals(refererUri.Host, Request.Host.Host, StringComparison.OrdinalIgnoreCase))
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