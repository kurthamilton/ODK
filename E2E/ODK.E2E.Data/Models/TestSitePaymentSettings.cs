namespace ODK.E2E.Data.Models;

/// <summary>
/// The <c>SitePaymentSettings</c> row a platform transacts through. <see cref="Id"/> is what a
/// <c>ChapterPaymentAccount</c> is attached to; <see cref="Name"/> is what the site-admin
/// create-subscription form picks it by, since that form offers payment settings by name; and
/// <see cref="ApiSecretKey"/> is for the few things a test does against Stripe directly (a test clock),
/// so those run against the same account the app itself is transacting on.
/// </summary>
public sealed record TestSitePaymentSettings(Guid Id, string Name, string ApiSecretKey);
