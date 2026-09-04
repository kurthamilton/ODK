using ODK.Core.Platforms;

namespace ODK.Infrastructure.Settings;

public class HangfireSettings
{
    public required bool InMemory { get; init; }

    public required Dictionary<PlatformType, HangfirePlatformSettings> Platforms { get; init; }

    public required int RetryAttempts { get; init; }

    public required int WorkerCount { get; init; }
}
