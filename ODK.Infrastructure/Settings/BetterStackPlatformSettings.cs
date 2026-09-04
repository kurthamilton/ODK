namespace ODK.Infrastructure.Settings;

public class BetterStackPlatformSettings
{
    /// <summary>
    /// The host that ingests this platform's logs, without a scheme
    /// (<c>s0000000.eu-central-1a.betterstackdata.com</c>). BetterStack issues one per source and recommends
    /// posting to it rather than to a shared endpoint, so it belongs to the source as much as the token does
    /// and is read alongside it.
    /// </summary>
    public required string IngestingHost { get; init; }

    /// <summary>
    /// The BetterStack source this platform's deployment ships its logs to. One source per platform, so a
    /// question about one platform is not read against the other's traffic.
    /// </summary>
    /// <remarks>
    /// Blank turns the sink off, as a blank <see cref="IngestingHost"/> does: a source is both together.
    /// </remarks>
    public required string SourceToken { get; init; }
}
