using Microsoft.AspNetCore.Mvc.RazorPages;
using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Web;
using ODK.Services;
using ODK.Web.Common.Extensions;
using ODK.Web.Common.Feedback;
using ODK.Web.Razor.Attributes;
using ODK.Web.Razor.Models;
using ODK.Web.Razor.Services;

namespace ODK.Web.Razor.Pages;

public abstract class OdkPageModel : PageModel
{
    public OdkComponentContext ComponentContext => RequestStore.ComponentContext;

    public Guid CurrentMemberId => RequestStore.CurrentMemberId;

    public Guid? CurrentMemberIdOrDefault => RequestStore.CurrentMemberIdOrDefault;

    public string? Description
    {
        get => ViewData["Description"] as string;
        set => ViewData["Description"] = value;
    }

    public IHttpRequestContext HttpRequestContext => RequestStore.HttpRequestContext;

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

    public MemberServiceRequest MemberServiceRequest => RequestStore.MemberServiceRequest;

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

    public MemberChapterServiceRequest CreateMemberChapterServiceRequest(Guid chapterId)
        => MemberChapterServiceRequest.Create(chapterId, MemberServiceRequest);

    public async Task<MemberChapterServiceRequest> CreateMemberChapterServiceRequest()
    {
        var chapter = await GetChapter();
        return CreateMemberChapterServiceRequest(chapter.Id);
    }

    public Task<Chapter> GetChapter() => RequestStore.GetChapter();

    public Task<Member> GetCurrentMember() => RequestStore.GetCurrentMember();

    protected void AddFeedback(FeedbackViewModel viewModel) => TempData!.AddFeedback(viewModel);

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
}