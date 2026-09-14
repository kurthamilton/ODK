namespace ODK.Services.Members;

/// <summary>
/// An invite's life after it is raised: withdrawing it, a member refusing it, and the retention purge
/// clearing it. Deliberately knows nothing about who is allowed to do any of that - a caller acting for an
/// admin authorises first and then calls this, never the other way round.
/// </summary>
public interface IMemberInviteService
{
    /// <summary>
    /// Withdraws a group's outstanding invite for one member, and with it the account an import raised to
    /// hold it where that invite was the only reason the account existed.
    /// </summary>
    Task<ServiceResult> CancelInvite(IChapterServiceRequest request, Guid memberId);

    Task<int> PurgeExpiredInvites();

    Task<ServiceResult> RefuseInvite(IChapterServiceRequest request, string token);
}
