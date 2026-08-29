using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Web;

namespace ODK.Services;

public class ServiceRequest : IServiceRequest
{
    public Guid? CurrentMemberIdOrDefault => CurrentMemberOrDefault?.Id;

    public required Member? CurrentMemberOrDefault { get; init; }

    public required EnvironmentType Environment { get; init; }

    public required IHttpRequestContext HttpRequestContext { get; init; }

    public required PlatformType Platform { get; init; }

    public static ServiceRequest Create(IServiceRequest other) => Create(other, other.Platform);

    public static ServiceRequest Create(IServiceRequest other, PlatformType platform) => new()
    {
        CurrentMemberOrDefault = other.CurrentMemberOrDefault,
        Environment = other.Environment,
        HttpRequestContext = other.HttpRequestContext,
        Platform = platform
    };
}