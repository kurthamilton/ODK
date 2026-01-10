using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Web;
using ODK.Services;
using ODK.Web.Razor.Models;

namespace ODK.Web.Razor.Services;

public interface IRequestStore
{
    OdkComponentContext ComponentContext { get; }

    Guid CurrentMemberId { get; }

    Guid? CurrentMemberIdOrDefault { get; }

    IHttpRequestContext HttpRequestContext { get; }

    MemberServiceRequest MemberServiceRequest { get; }

    PlatformType Platform { get; }

    ServiceRequest ServiceRequest { get; }

    Task<Chapter> GetChapter();

    Task<Chapter?> GetChapterOrDefault();

    Task<Member> GetCurrentMember();

    Task<Member?> GetCurrentMemberOrDefault();
}