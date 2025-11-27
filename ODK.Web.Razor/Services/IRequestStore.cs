using ODK.Core.Chapters;
using ODK.Core.Platforms;
using ODK.Core.Web;
using ODK.Web.Razor.Models;

namespace ODK.Web.Razor.Services;

public interface IRequestStore
{
    Chapter? Chapter { get; }

    OdkComponentContext ComponentContext { get; }

    Guid CurrentMemberId { get; }

    Guid? CurrentMemberIdOrDefault { get; }

    IHttpRequestContext HttpRequestContext { get; }    

    PlatformType Platform { get; }
}
