namespace ODK.Services.Members;

public interface IMemberInviteService
{
    Task<int> PurgeExpiredInvites();

    Task<ServiceResult> RefuseInvite(IChapterServiceRequest request, string token);
}
