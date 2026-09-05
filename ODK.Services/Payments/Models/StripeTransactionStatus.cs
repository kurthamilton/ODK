namespace ODK.Services.Payments.Models;

/// <summary>
/// Whether a transaction took money. Only <see cref="Succeeded"/> is audited: one that took nothing has
/// nothing to reconcile against, and judging it would report a finding against an attempt.
/// </summary>
public enum StripeTransactionStatus
{
    None = 0,

    /// <summary>Abandoned, voided or written off. Nothing more will happen to it.</summary>
    Cancelled,

    /// <summary>Still open - awaiting payment, or being processed.</summary>
    Pending,

    Succeeded
}
