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

    /// <summary>
    /// Emails an outstanding invite again to whoever asks for it by address, from a page that does not
    /// know who they are. Succeeds identically whether or not there was anything to send - the caller is
    /// anonymous, so telling it apart would tell anyone who was in this group.
    /// </summary>
    Task<ServiceResult> RequestInviteResend(IChapterServiceRequest request, string emailAddress);

    /// <summary>
    /// Emails one invite. The group's admin service queues this for a batch; it lives here because what an
    /// invite email is made of is the same question however the send was asked for.
    /// </summary>
    Task SendInviteEmail(IServiceRequest request, Guid chapterId, Guid memberId);
}
