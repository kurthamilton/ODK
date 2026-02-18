namespace ODK.Infrastructure.Settings;

public class HangfireSettings
{
    public required bool InMemory { get; init; }

    public required int RetryAttempts { get; init; }

    public required string SchemaName { get; init; }

    public required int WorkerCount { get; init; }
}
