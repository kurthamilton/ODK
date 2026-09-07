using Microsoft.AspNetCore.Mvc.RazorPages;
using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Services;
using ODK.Web.Common.Routes;
using ODK.Web.Common.Services;
using ODK.Web.Razor.Attributes;
using ODK.Web.Razor.Extensions;
using ODK.Web.Razor.Models.Feedback;

namespace ODK.Web.Razor.Pages;

public abstract class OdkPageModel : PageModel
{
    public Chapter Chapter => RequestStore.Chapter;

    /// <summary>
    /// The request's chapter where it has one. A site-level page - anything outside the chapter-scoped route
    /// trees - has none on Group Squirrel, and <see cref="Chapter"/> throws rather than returning null, so a
    /// page that can be reached either way reads it from here.
    /// </summary>
    public Chapter? ChapterOrDefault => RequestStore.ChapterOrDefault;

    public IChapterServiceRequest ChapterServiceRequest => RequestStore.ChapterServiceRequest;

    public Member CurrentMember => RequestStore.CurrentMember;

    public Member? CurrentMemberOrDefault => RequestStore.CurrentMemberOrDefault;

    public string? Description
    {
        get => ViewData.Description;
        set => ViewData.Description = value;
    }

    public ILocation? Location
    {
        get => ViewData.Location;
        set => ViewData.Location = value;
    }

    public IReadOnlyCollection<string>? Keywords
    {
        get => ViewData.Keywords;
        set => ViewData.Keywords = value;
    }

    public IMemberChapterServiceRequest MemberChapterServiceRequest => RequestStore.MemberChapterServiceRequest;

    public IMemberServiceRequest MemberServiceRequest => RequestStore.MemberServiceRequest;

    [OdkInject]
    public required IOdkRoutes OdkRoutes { get; set; }

    public string? Path
    {
        get => ViewData.Path;
        set => ViewData.Path = value;
    }

    public PlatformType Platform => RequestStore.Platform;

    [OdkInject]
    public required IRequestStore RequestStore { get; set; }

    public IServiceRequest ServiceRequest => RequestStore.ServiceRequest;

    // Set by a page that reads the IP location database. The footer renders the DB-IP credit its CC BY
    // licence requires on pages using the data.
    public bool ShowGeoIpAttribution
    {
        get => ViewData.ShowGeoIpAttribution;
        set => ViewData.ShowGeoIpAttribution = value;
    }

    public string? Title
    {
        get => ViewData.Title;
        set => ViewData.Title = value;
    }

    protected void AddFeedback(string message, FeedbackType type = FeedbackType.Success)
        => AddFeedback(new FeedbackViewModel(message, type));

    /// <summary>
    /// One item of feedback per message the result carries, so a save that found several problems reports
    /// all of them rather than the first alone.
    /// </summary>
    protected void AddFeedback(ServiceResult result)
    {
        var type = result.Success ? FeedbackType.Success : FeedbackType.Error;
        foreach (var message in result.Messages)
        {
            AddFeedback(message, type);
        }
    }

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

    private void AddFeedback(FeedbackViewModel viewModel) => TempData!.AddFeedback(viewModel);
}