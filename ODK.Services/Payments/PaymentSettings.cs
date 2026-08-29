using ODK.Core.Payments;
using ODK.Core.Platforms;

namespace ODK.Services.Payments;

/// <summary>
/// What the running deployment transacts as. Shared rather than declared per service, because "which
/// account is this platform on" is one fact several services ask.
/// </summary>
/// <remarks>
/// Only a <em>new</em> entity takes its environment, platform and provider from here - by way of the
/// service request. Everything already written carries its own, so a record is read back under the account
/// it was written under and never under whichever one config names today.
/// </remarks>
public class PaymentSettings
{
    public required IReadOnlyDictionary<PlatformType, PaymentPlatformSettings> Platforms { get; init; }

    public required PaymentProviderType Provider { get; init; }

    public PaymentPlatformSettings GetPlatform(PlatformType platform) => Platforms[platform];

    /// <summary>
    /// The account the platform transacts as, or null where config names none. Null is a deployment that
    /// cannot take a payment on that platform, not a fault.
    /// </summary>
    public PaymentPlatformSettings? GetPlatformOrDefault(PlatformType platform)
        => Platforms.TryGetValue(platform, out var settings) ? settings : null;

    /// <summary>Whether a payment can be taken on the platform at all.</summary>
    public bool IsEnabled(PlatformType platform) => GetPlatformOrDefault(platform)?.Enabled == true;
}
