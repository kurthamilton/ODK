namespace ODK.Services.Logging;

public class LoggingServiceSettings
{
    public required IReadOnlyCollection<IgnoreExceptionRule> IgnoreExceptions { get; init; } = [];

    /// <summary>
    /// How long log rows are kept before <c>PurgeLogs</c> deletes them. The privacy policy states this
    /// period, so both read it from <c>Privacy:Logging:DefaultRetentionDays</c>.
    /// </summary>
    public required int LogRetentionDays { get; init; }
}