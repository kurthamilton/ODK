using ODK.Core.Platforms;
using ODK.Core.Web;

namespace ODK.Web.Razor.Models;

public class OdkComponentContext
{
    public required Guid? CurrentMemberIdOrDefault { get; init; }

    public required IHttpRequestContext HttpRequestContext { get; init; }

    public required PlatformType Platform { get; init; }
}
