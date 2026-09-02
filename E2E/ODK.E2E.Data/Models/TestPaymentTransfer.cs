namespace ODK.E2E.Data.Models;

/// <summary>
/// What a payment records about moving a group's share: the share the settlement stated, how much of it was
/// withheld against what the group owed, the provider's transfer of whatever was left, and when it happened.
/// </summary>
/// <remarks>
/// The share and the withheld amount are read as a pair deliberately - what was sent is the difference
/// between them, and no column states it.
/// </remarks>
public sealed record TestPaymentTransfer(
    decimal? ConnectedAccountAmount,
    string? ExternalTransferId,
    decimal? WithheldAmount,
    DateTime? TransferredUtc);
