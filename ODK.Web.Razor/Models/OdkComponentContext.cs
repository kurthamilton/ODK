using ODK.Core.Platforms;
using ODK.Core.Web;
using ODK.Services.Exceptions;

namespace ODK.Web.Razor.Models;

public class OdkComponentContext
{
    public Guid CurrentMemberId => CurrentMemberIdOrDefault ?? throw new OdkNotAuthorizedException();

    public required Guid? CurrentMemberIdOrDefault { get; init; }

    public required IHttpRequestContext HttpRequestContext { get; init; }

    public bool IsAuthenticated => CurrentMemberIdOrDefault != null;

    public required PlatformType Platform { get; init; }
}
