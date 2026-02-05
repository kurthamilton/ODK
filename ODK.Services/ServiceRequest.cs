using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Web;

namespace ODK.Services;

public class ServiceRequest : IServiceRequest
{
    public Guid? CurrentMemberIdOrDefault => CurrentMemberOrDefault?.Id;

    public required Member? CurrentMemberOrDefault { get; init; }

    public required IHttpRequestContext HttpRequestContext { get; init; }

    public required PlatformType Platform { get; init; }

    public static ServiceRequest Create(IServiceRequest other) => new()
    {
        CurrentMemberOrDefault = other.CurrentMemberOrDefault,
        HttpRequestContext = other.HttpRequestContext,
        Platform = other.Platform
    };
}