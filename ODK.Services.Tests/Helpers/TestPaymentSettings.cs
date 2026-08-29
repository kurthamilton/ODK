using System.Collections.Generic;
using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Services.Payments;

namespace ODK.Services.Tests.Helpers;

internal static class TestPaymentSettings
{
    /// <summary>
    /// A deployment that transacts on Stripe as development, on both platforms. The environment matches what
    /// <see cref="MockOdkContext"/> stamps its records with, so a record it creates is one this deployment
    /// can read back.
    /// </summary>
    internal static PaymentSettings Create(bool enabled = true) => new()
    {
        Platforms = new Dictionary<PlatformType, PaymentPlatformSettings>
        {
            [PlatformType.Default] = Platform(enabled),
            [PlatformType.DrunkenKnitwits] = Platform(enabled)
        },
        Provider = PaymentProviderType.Stripe
    };

    private static PaymentPlatformSettings Platform(bool enabled) => new()
    {
        AccountId = "acct_test",
        Enabled = enabled,
        PublicApiKey = "pk_test_dummy"
    };
}
