namespace ODK.Services.Payments.Models;

/// <summary>
/// What an unmet check costs. Read only where the state is
/// <see cref="StripeWebhookCheckState.Unmet"/> - a check that passed or could not be made has nothing to
/// weigh.
/// </summary>
public enum StripeWebhookCheckSeverity
{
    None = 0,

    /// <summary>Worth knowing, and working as it is.</summary>
    Info,

    /// <summary>Working, but by something the endpoint does not state - a Drunken Knitwits URL with no <c>p</c>.</summary>
    Warning,

    /// <summary>Events are being lost, or will be.</summary>
    Error
}
