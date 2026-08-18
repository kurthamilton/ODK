using ODK.Core.Platforms;
using ODK.Core.Workflows;

namespace ODK.Services.Members.Workflows.Guards;

/// <summary>
/// Restricts an edge to one platform, so a difference between them is a visible branch in the graph
/// rather than a switch inside a step.
/// </summary>
public sealed class OnPlatform : IGuard<AccountContext>
{
    private readonly PlatformType _platform;

    public OnPlatform(PlatformType platform)
    {
        _platform = platform;
    }

    public string Description => $"on {_platform}";

    public bool IsSatisfied(AccountContext context) => context.Platform == _platform;
}
