using ODK.Core.Platforms;
using ODK.Core.Web;

namespace ODK.Services;

public class ServiceRequest : IServiceRequest
{
    public virtual required Guid? CurrentMemberIdOrDefault { get; init; }

    public required IHttpRequestContext HttpRequestContext { get; init; }

    public required PlatformType Platform { get; init; }

    public static ServiceRequest Create(IServiceRequest other) => new()
    {
        CurrentMemberIdOrDefault = other.CurrentMemberIdOrDefault,
        HttpRequestContext = other.HttpRequestContext,
        Platform = other.Platform
    };
}