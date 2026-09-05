using System.Diagnostics.CodeAnalysis;
using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Members;

namespace ODK.Services.Payments.Models;

/// <summary>
/// One look at a Stripe account: what it holds, what our records make of it, and the rows behind the names.
/// </summary>
/// <remarks>
/// Shared by the overview page and by the actions on it, so a row action is decided by the same reading the
/// page showed rather than by a second one of its own - a row action working the question out for itself
/// could act on a transaction the page never offered.
/// </remarks>
public class StripeAccountRead
{
    public required StripePaymentAccount Account { get; init; }

    public required StripeAccountAuditResult? Audit { get; init; }

    /// <summary>The groups our records and the account's metadata name, for resolving names.</summary>
    public required IReadOnlyCollection<Chapter> Chapters { get; init; }

    public required IReadOnlyCollection<Currency> Currencies { get; init; }

    /// <summary>
    /// Why the account could not be read, where it could not be - a revoked key, a rejected request, a
    /// network failure.
    /// </summary>
    public required string? Error { get; init; }

    public required IReadOnlyCollection<Member> Members { get; init; }

    public required StripeTransactionRecords? Records { get; init; }

    /// <summary>
    /// Whether the account answered. False means <see cref="Error"/> says why and nothing else here is
    /// stated - an account that could not be listed is not an account holding none of our payments.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Audit), nameof(Records))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool Readable => Error == null;
}
