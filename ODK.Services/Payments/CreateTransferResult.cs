namespace ODK.Services.Payments;

/// <summary>
/// The outcome of moving a payment's share on to a group's connected account.
/// </summary>
/// <remarks>
/// A named result rather than a <see cref="ServiceResult{T}"/> of string: the payload is the provider's
/// transfer id, and only its property name can say so. Reversing that transfer is the only way money is
/// taken back from a group, so the id has to reach the caller to be recorded rather than being discarded
/// with the rest of the provider's response.
/// </remarks>
public class CreateTransferResult : ServiceResult
{
    private CreateTransferResult(bool success, string? message = null, string? externalTransferId = null)
        : base(success, message)
    {
        ExternalTransferId = externalTransferId;
    }

    /// <summary>
    /// The transfer the provider made. Null on failure.
    /// </summary>
    public string? ExternalTransferId { get; }

    /// <summary>
    /// The money did not move, and the caller is expected to raise the failure rather than record anything.
    /// </summary>
    public new static CreateTransferResult Failure(string message) => new(false, message);

    /// <summary>
    /// The money moved, on the transfer named by <paramref name="externalTransferId"/>.
    /// </summary>
    public static CreateTransferResult Transferred(string externalTransferId)
        => new(true, externalTransferId: externalTransferId);
}
