using Microsoft.AspNetCore.Mvc.RazorPages;
using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Services;
using ODK.Web.Common.Extensions;
using ODK.Web.Common.Feedback;
using ODK.Web.Common.Routes;
using ODK.Web.Common.Services;
using ODK.Web.Razor.Attributes;

namespace ODK.Web.Razor.Pages;

public abstract class OdkPageModel : PageModel
{
    public Chapter Chapter => RequestStore.Chapter;

    public ChapterServiceRequest ChapterServiceRequest => RequestStore.ChapterServiceRequest;

    public Member CurrentMember => RequestStore.CurrentMember;

    public Guid CurrentMemberId => RequestStore.CurrentMemberId;

    public Guid? CurrentMemberIdOrDefault => RequestStore.CurrentMemberIdOrDefault;

    public Member? CurrentMemberOrDefault => RequestStore.CurrentMemberOrDefault;

    public string? Description
    {
        get => ViewData["Description"] as string;
        set => ViewData["Description"] = value;
    }

    public ILocation? Location
    {
        get => ViewData["Location"] as ILocation;
        set => ViewData["Location"] = value;
    }

    public IReadOnlyCollection<string>? Keywords
    {
        get => ViewData["Keywords"] as IReadOnlyCollection<string>;
        set => ViewData["Keywords"] = value;
    }

    public MemberChapterServiceRequest MemberChapterServiceRequest => RequestStore.MemberChapterServiceRequest;

    public MemberServiceRequest MemberServiceRequest => RequestStore.MemberServiceRequest;

    [OdkInject]
    public required IOdkRoutes OdkRoutes { get; set; }

    public string? Path
    {
        get => ViewData["Path"] as string;
        set => ViewData["Path"] = value;
    }

    public PlatformType Platform => RequestStore.Platform;

    [OdkInject]
    public required IRequestStore RequestStore { get; set; }

    public ServiceRequest ServiceRequest => RequestStore.ServiceRequest;

    public string? Title
    {
        get => ViewData["Title"] as string;
        set => ViewData["Title"] = value;
    }        

    protected void AddFeedback(string message, FeedbackType type = FeedbackType.Success)
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

    private void AddFeedback(FeedbackViewModel viewModel) => TempData!.AddFeedback(viewModel);
}