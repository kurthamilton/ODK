using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Utils;
using ODK.Services;
using ODK.Web.Common.Extensions;
using ODK.Web.Common.Feedback;
using ODK.Web.Common.Routes;
using ODK.Web.Common.Services;

namespace ODK.Web.Razor.Controllers;

public abstract class OdkControllerBase : Controller
{
    private static readonly Regex VersionRegex = new(@"^""(?<version>-?\d+)""$");

    private readonly IRequestStore _requestStore;

    protected OdkControllerBase(
        IRequestStore requestStore,
        IOdkRoutes odkRoutes)
    {
        _requestStore = requestStore;
        OdkRoutes = odkRoutes;
    }

    protected Chapter Chapter => _requestStore.Chapter;

    protected Member CurrentMember => _requestStore.CurrentMember;

    protected MemberChapterServiceRequest MemberChapterServiceRequest => _requestStore.MemberChapterServiceRequest;

    protected Guid MemberId => _requestStore.CurrentMemberId;

    protected MemberServiceRequest MemberServiceRequest => _requestStore.MemberServiceRequest;

    protected IOdkRoutes OdkRoutes { get; }

    protected PlatformType Platform => _requestStore.Platform;

    protected ServiceRequest ServiceRequest => _requestStore.ServiceRequest;

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

    protected IActionResult DownloadCsv(IReadOnlyCollection<IReadOnlyCollection<string>> data, string fileName)
    {
        var csv = StringUtils.ToCsv(data);
        return File(Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
    }    

    protected async Task<IActionResult> HandleVersionedRequest<T>(Func<long?, Task<VersionedServiceResult<T>>> getter, Func<T?, IActionResult> map) where T : class
    {
        long? version = GetRequestVersion();

        var result = await getter(version);

        AddVersionHeader(result.Version);

        if (version == result.Version)
        {
            return new StatusCodeResult(304);
        }

        return map(result.Value);
    }

    protected async Task<string> ReadBodyText()
    {
        using var reader = new StreamReader(Request.Body);
        var text = await reader.ReadToEndAsync();
        return text;
    }

    protected IActionResult RedirectToReferrer(string? fallback = null)
    {
        var url = Request.Headers["Referer"].ToString();
        if (string.IsNullOrEmpty(url))
        {
            url = !string.IsNullOrEmpty(fallback)
                ? fallback
                : Request.Path;
        }

        return Redirect(url);
    }

    private void AddVersionHeader(long version)
    {
        Response.Headers.Append("ETag", $"\"{version}\"");
    }

    private long? GetRequestVersion()
    {
        string? requestETag = Request.Headers["If-None-Match"].FirstOrDefault();
        if (requestETag == null)
        {
            return null;
        }

        var match = VersionRegex.Match(requestETag);
        return match.Success ? long.Parse(match.Groups["version"].Value) : new long?();
    }
}