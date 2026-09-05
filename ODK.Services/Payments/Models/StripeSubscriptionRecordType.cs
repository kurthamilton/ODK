namespace ODK.Services.Payments.Models;

/// <summary>
/// Which of our two subscription records a Stripe subscription bills. Stripe has one concept where we have
/// two tables, so a row of the overview has to say which it came from.
/// </summary>
public enum StripeSubscriptionRecordType
{
    None = 0,

    /// <summary>A member's subscription to a group - <c>MemberSubscriptionRecord</c>.</summary>
    Group,

    /// <summary>A member's subscription to the site - <c>MemberSiteSubscriptionRecord</c>.</summary>
    Site
}
