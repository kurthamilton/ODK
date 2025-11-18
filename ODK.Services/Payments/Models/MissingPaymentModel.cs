using ODK.Core.Countries;
using ODK.Core.Members;

namespace ODK.Services.Payments.Models;

public class MissingPaymentModel
{
    public required decimal Amount { get; init; }

    public required Currency Currency { get; init; }

    public required DateTime Created { get; init; }

    public required Member? Member { get; init; }

    public required string? MemberEmail { get; init; }

    public required MemberSubscriptionRecord? MemberSubscriptionRecord { get; init; }

    public required string PaymentId { get; init; }

    public required string? SubscriptionId { get; init; }
}
