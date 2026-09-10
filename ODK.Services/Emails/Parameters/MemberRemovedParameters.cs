namespace ODK.Services.Emails.Parameters;

/// <summary>
/// Tells a member an organiser has removed them from a group.
/// </summary>
public sealed class MemberRemovedParameters : EmailTypeParameters
{
    private const string ReasonName = "member.removedReason";

    public static IReadOnlyCollection<string> Names { get; } = [ReasonName];

    public required string? Reason { get; init; }

    protected override void AddParameters(IDictionary<string, string> values)
    {
        Add(values, ReasonName, !string.IsNullOrEmpty(Reason) ? Reason : NoValue);
    }
}
